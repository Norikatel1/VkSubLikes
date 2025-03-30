using System.Diagnostics;
using System.Text.Json;
using WinFormsApp1.Models;
using static WinFormsApp1.Models.MembersModel;

namespace WinFormsApp1.API
{
    public class VkApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "https://api.vk.com/method";
        private readonly JsonSerializerOptions _jsonOptions;
        private const string VkApiVersion = "5.199";
        private const int MaxItemsPerRequest = 1000;
        private const int MaxRetryAttempts = 3;
        private const int RetryDelayMs = 500;

        public VkApiClient(string accessToken)
        {
            _httpClient = new HttpClient
            {
                DefaultRequestHeaders =
                {
                    Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken)
                }
            };

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };
        }

        public async Task<MembersApiResponse> GetMembers(int ownerId)
        {
            var allMembers = new List<long>();
            int offset = 0;
            bool hasMore = true;

            while (hasMore)
            {
                var queryString = BuildMembersQueryString(ownerId, offset);
                var response = await MakeApiRequest<MembersApiResponse>("groups.getMembers", queryString);

                if (response?.Response?.Items == null)
                    break;

                allMembers.AddRange(response.Response.Items);
                offset += MaxItemsPerRequest;
                hasMore = response.Response.Items.Count == MaxItemsPerRequest;

                Debug.WriteLine($"Current response: {response.Response.Items.Count}, Total members: {allMembers.Count}");
            }

            return new MembersApiResponse
            {
                Response = new MembersResponseData
                {
                    Count = allMembers.Count,
                    Items = allMembers
                }
            };
        }

        public async Task<GetLikeApiResponse> GetLikes(int ownerId, int itemId)
        {
            var allLikes = new List<User>();
            int offset = 0;
            bool hasMore = true;

            while (hasMore)
            {
                var queryString = BuildLikesQueryString(ownerId, itemId, offset);
                var response = await MakeApiRequest<GetLikeApiResponse>("likes.getList", queryString);

                if (response?.Response?.Items == null)
                    break;

                allLikes.AddRange(response.Response.Items);
                offset += MaxItemsPerRequest;
                hasMore = response.Response.Items.Count == MaxItemsPerRequest;
            }

            return new GetLikeApiResponse
            {
                Response = new GetLikeResponseData
                {
                    Count = allLikes.Count,
                    Items = allLikes
                }
            };
        }

        private string BuildMembersQueryString(int ownerId, int offset)
        {
            return $"group_id={ownerId}" +
                   $"&v={VkApiVersion}" +
                   $"&count={MaxItemsPerRequest}" +
                   $"&offset={offset}";
        }

        private string BuildLikesQueryString(int ownerId, int itemId, int offset)
        {
            return $"type=post" +
                   $"&owner_id={-ownerId}" +
                   $"&item_id={itemId}" +
                   $"&v={VkApiVersion}" +
                   $"&count={MaxItemsPerRequest}" +
                   $"&extended=1" +
                   $"&offset={offset}";
        }

        private async Task<T> MakeApiRequest<T>(string method, string queryString)
        {
            int currentRetry = 0;

            while (currentRetry < MaxRetryAttempts)
            {
                try
                {
                    string apiUrl = $"{_baseUrl}/{method}?{queryString}";
                    var httpContent = new StringContent("application/x-www-form-urlencoded");
                    var response = await _httpClient.PostAsync(apiUrl, httpContent);
                    string json = await response.Content.ReadAsStringAsync();

                    if (json.Contains("Too many request", StringComparison.OrdinalIgnoreCase))
                    {
                        Debug.WriteLine("Too many requests - retrying...");
                        currentRetry++;
                        await Task.Delay(RetryDelayMs);
                        continue;
                    }

                    return JsonSerializer.Deserialize<T>(json, _jsonOptions);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"API request failed: {ex.Message}");
                    throw new ApplicationException($"API request failed: {ex.Message}", ex);
                }
            }

            throw new ApplicationException($"API request failed after {MaxRetryAttempts} attempts");
        }
    }
}
