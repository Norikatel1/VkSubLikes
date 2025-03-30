using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WinFormsApp1.Models
{
    public class MembersModel
    {
        public class MembersApiResponse
        {
            [JsonPropertyName("response")]
            public MembersResponseData Response { get; set; }
        }

        public class MembersResponseData
        {
            [JsonPropertyName("count")]
            public int Count { get; set; }

            [JsonPropertyName("items")]
            public List<long> Items { get; set; }

            [JsonPropertyName("next_from")]
            public string NextFrom { get; set; }
        }
    }
}
