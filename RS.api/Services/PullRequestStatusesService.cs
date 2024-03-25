using Microsoft.Extensions.Configuration;
using RS.api.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RS.api.Services
{
    public interface IPullRequestStatusesService
    {
        Task<PullRequestStatusResponse> GetStatus(int pullRequestId);
        void RemoveStatus(int pullRequestId);
        Task SetStatusAsync(int pullRequestId, PullRequestStatusParam model);
    }

    public class PullRequestStatusesService : IPullRequestStatusesService
    {
        private string token = "";
        private string organizationName = "";
        private string projectName = "";
        private string repoId = "";
        private string prUrl = "";

        public PullRequestStatusesService(IConfiguration configuration)
        {
            string pat = configuration.GetSection("AppSettings").GetSection("PAT").Value;

            token = string.Format("{0}:{1}", "", pat);
            organizationName = configuration.GetSection("AppSettings").GetSection("OrganizationName").Value;
            projectName = configuration.GetSection("AppSettings").GetSection("ProjectName").Value;
            repoId = configuration.GetSection("AppSettings").GetSection("RepoCode").Value;

            prUrl = "https://dev.azure.com/" + organizationName + "/" + projectName + "/_apis/git/repositories/" + repoId + "/pullRequests/";

        }

        public async Task<PullRequestStatusResponse> GetStatus(int pullRequestId)
        {
            PullRequestStatusResponse prs = null;
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(token)));

                using (HttpResponseMessage response = client.GetAsync(prUrl + pullRequestId + "/statuses/1?api-version=5.0-preview.1").Result)
                {
                    if (response.StatusCode != System.Net.HttpStatusCode.NotFound)
                    {
                        response.EnsureSuccessStatusCode();
                        string responseBody = await response.Content.ReadAsStringAsync();
                        prs = JsonSerializer.Deserialize<PullRequestStatusResponse>(responseBody);
                    }
                }
            }

            return prs;
        }

        public async Task SetStatusAsync(int pullRequestId, PullRequestStatusParam model)
        {
            PullRequestStatusResponse respStatus = await GetStatus(pullRequestId);

            if (respStatus == null)
                respStatus = new PullRequestStatusResponse();

            if (respStatus.state != model.state)
            {
                if (respStatus.state != null)
                    RemoveStatus(pullRequestId);

                using (HttpClient client = new HttpClient())
                {
                    Dictionary<string, string> p = new Dictionary<string, string>();

                    string jsonRequest = JsonSerializer.Serialize(model);
                    var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                        Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(token)));

                    using (HttpResponseMessage response = client.PostAsync(prUrl + pullRequestId + "/statuses?api-version=5.0-preview.1", content).Result)
                    {
                        response.EnsureSuccessStatusCode();
                    }
                }
            }
        }

        public void RemoveStatus(int pullRequestId)
        {
            using (HttpClient client = new HttpClient())
            {
                Dictionary<string, string> p = new Dictionary<string, string>();

                string jsonRequest = "[{\"op\": \"remove\", \"path\": \"/1\"}]";
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json-patch+json");

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(token)));

                using (HttpResponseMessage response = client.PatchAsync(prUrl + pullRequestId + "/statuses?api-version=5.0-preview.1", content).Result)
                {
                    response.EnsureSuccessStatusCode();
                }
            }
        }
    }
}
