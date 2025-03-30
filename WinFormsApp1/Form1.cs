using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using WinFormsApp1.API;
using WinFormsApp1.Models;
using static WinFormsApp1.Models.MembersModel;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        private readonly VkApiClient _vkApiClient;

        public Form1()
        {
            InitializeComponent();
            _vkApiClient = new VkApiClient("Your_API_Token");
        }

        private async void toolStripButton1_Click(object sender, EventArgs e)
        {
            try
            {
                if (!ValidateUrl(toolStripTextBox1.Text, out var postInfo))
                {
                    return;
                }
                toolStripProgressBar1.Visible = true;
                await LoadAndDisplayPostData(postInfo.ownerId, postInfo.itemId);
                toolStripProgressBar1.Visible = false;
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"An error occurred: {ex.Message}");
            }
        }

        private bool ValidateUrl(string url, out (int ownerId, int itemId) postInfo)
        {
            postInfo = (0, 0);

            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult))
            {
                ShowErrorMessage("Invalid URL format");
                return false;
            }

            postInfo = ParsePostUrl(uriResult);
            if (postInfo.ownerId == 0 || postInfo.itemId == 0)
            {
                ShowErrorMessage("Could not extract post information from URL");
                return false;
            }

            return true;
        }

        private async Task LoadAndDisplayPostData(int ownerId, int itemId)
        {
            var likesResponse = await _vkApiClient.GetLikes(ownerId, itemId);
            if (likesResponse?.Response?.Items == null)
            {
                ShowErrorMessage("Failed to get likes data");
                return;
            }

            DisplayLikesData(likesResponse.Response.Items);

            var members = await _vkApiClient.GetMembers(ownerId);
            if (members?.Response?.Items != null)
            {
                DisplaySubscribersWhoLiked(likesResponse.Response.Items, members.Response.Items);
            }
        }

        private void DisplayLikesData(IReadOnlyCollection<User> usersWhoLiked)
        {
            dataGridView1.DataSource = usersWhoLiked.ToList();
            toolStripLabel1.Text = $"Лайкнувших - {usersWhoLiked.Count}";
        }

        private void DisplaySubscribersWhoLiked(IEnumerable<User> usersWhoLiked, ICollection<long> memberIds)
        {
            var subscribersWhoLiked = usersWhoLiked
                .Where(user => memberIds.Contains(user.Id))
                .ToList();

            Debug.WriteLine($"Total members: {memberIds.Count}");
            toolStripLabel2.Text = $"Из них подписчики - {subscribersWhoLiked.Count}";
            dataGridView2.DataSource = subscribersWhoLiked;
        }

        private (int ownerId, int itemId) ParsePostUrl(Uri uri)
        {
            var match = Regex.Match(uri.AbsoluteUri, @"wall-(\d+)_(\d+)");

            if (match.Success &&
                int.TryParse(match.Groups[1].Value, out int ownerId) &&
                int.TryParse(match.Groups[2].Value, out int itemId))
            {
                return (ownerId, itemId);
            }
            return (0, 0);
        }

        private void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }


}