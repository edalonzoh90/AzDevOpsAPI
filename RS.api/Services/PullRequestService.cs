using Microsoft.Extensions.Configuration;
using RS.api.Models;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace RS.api.Services
{
    public interface IPullRequestService
    {
        Task<PullRequestResponse> GetWorkItems(PullRequestParam param);
        Task<PullRequestResponse> GetBDWorkItems(PullRequestParam param);
        Task<bool> ValidateWorkItemStatusAsync(PullRequestParam param);
        Task<bool> ValidateWorkItemTargetDateAsync(PullRequestParam param);
    }

    public class PullRequestService : IPullRequestService
    {

        private string token = "";
        private string organizationName = "";
        private string projectName = "";
        private string repoId = "";
        private string gitBaseUrl = "";

        static private string repoBDId = "";
        static private string gitBaseBDUrl = "";

        private readonly IWorkItemService _workItemService;
        private readonly IConfiguration configuration;



        public PullRequestService(IConfiguration configuration, IWorkItemService workItemService)
        {
            this.configuration = configuration;

            string pat = configuration.GetSection("AppSettings").GetSection("PAT").Value;

            token = string.Format("{0}:{1}", "", pat);
            organizationName = configuration.GetSection("AppSettings").GetSection("OrganizationName").Value;
            projectName = configuration.GetSection("AppSettings").GetSection("ProjectName").Value;
            repoId = configuration.GetSection("AppSettings").GetSection("RepoCode").Value;
            repoBDId = configuration.GetSection("AppSettings").GetSection("RepoDB").Value;

            gitBaseUrl = String.Format("https://dev.azure.com/{0}/{1}/_apis/git/repositories/{2}/pullRequests/", organizationName, projectName, repoId);
            gitBaseBDUrl = String.Format("https://dev.azure.com/{0}/{1}/_apis/git/repositories/{2}/pullRequests/", organizationName, projectName, repoBDId);

            _workItemService = workItemService;
        }

        public async Task<PullRequestResponse> GetWorkItems(PullRequestParam param)
        {
            PullRequestResponse pr = null;
            string url = string.Format("{0}/{1}/workitems", gitBaseUrl, param.resource.pullRequestId);

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(token)));

                using (HttpResponseMessage response = client.GetAsync(url).Result)
                {
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    pr = JsonSerializer.Deserialize<PullRequestResponse>(responseBody);
                }
            }

            return pr;
        }

        public async Task<PullRequestResponse> GetBDWorkItems(PullRequestParam param)
        {
            PullRequestResponse pr = null;
            string url = string.Format("{0}/{1}/workitems", gitBaseBDUrl, param.resource.pullRequestId);

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(token)));

                using (HttpResponseMessage response = client.GetAsync(url).Result)
                {
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    pr = JsonSerializer.Deserialize<PullRequestResponse>(responseBody);
                }
            }

            return pr;
        }

        public async Task<bool> ValidateWorkItemStatusAsync(PullRequestParam param)
        {
            WorkItemResponse wiR = null;
            bool wiStatus = true;

            PullRequestResponse pr = await GetWorkItems(param);

            if (pr.value.Count == 0)
                wiStatus = false;

            foreach (WorkItem wi in pr.value)
            {
                wiR = await _workItemService.GetDetailAsync(int.Parse(wi.id));

                if (param.resource.status != "completed"
                    && (wiR.fields.SystemWorkItemType == "Bug" || wiR.fields.SystemWorkItemType == "Task")
                    && (wiR.fields.SystemState != "Dev Review" && wiR.fields.SystemState != "Waiting QA Deployment"))
                {
                    wiStatus = false;
                    break;
                }
            }

            return wiStatus;
        }

        public async Task<bool> ValidateWorkItemTargetDateAsync(PullRequestParam param)
        {
            SettingsModel settings = new SettingsModel(configuration);

            WorkItemResponse wiR = null;
            bool wiStatus = true;

            PullRequestResponse pr = await GetWorkItems(param);

            if (pr.value.Count == 0)
                wiStatus = false;

            foreach (WorkItem wi in pr.value)
            {
                wiR = await _workItemService.GetDetailAsync(int.Parse(wi.id));


                if (param.resource.status != "completed"
                    && (wiR.fields.SystemWorkItemType == "Bug" || wiR.fields.SystemWorkItemType == "Task")
                    && (wiR.fields.TargetDate.Date != settings.ReleaseDate.Date))
                {
                    wiStatus = false;
                    break;
                }
            }

            return wiStatus;
        }
    }
}
