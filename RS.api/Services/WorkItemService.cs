using Microsoft.Extensions.Configuration;
using RS.api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RS.api.Services
{
    public interface IWorkItemService
    {
        Task<WorkItemResponse> GetDetailAsync(int workItemId);
        Task<List<WorkItemResponse>> GetChildsAsync(int workItemId);
        Task<List<string>> GetPullRequestAsync(int workItemId);
        Task UpdateToDevReviewAsync(string workItemId);
        Task UpdateToQAAsync(string workItemId);
    }

    public class WorkItemService : IWorkItemService
    {
        string token = string.Format("");
        private string organizationName = "";
        private string projectName = "";
        private string wipatch = "";
        private string prurl = "";

        public WorkItemService(IConfiguration configuration)
        {
            string pat = configuration.GetSection("AppSettings").GetSection("PAT").Value;

            token = string.Format("{0}:{1}", "", pat);
            organizationName = configuration.GetSection("AppSettings").GetSection("OrganizationName").Value;
            projectName = configuration.GetSection("AppSettings").GetSection("ProjectName").Value;

            wipatch = string.Format("https://dev.azure.com/{0}/_apis/wit/workitems/", organizationName, projectName);
            prurl = string.Format("https://dev.azure.com/{0}/{1}/_apis/git/pullrequests/", organizationName, projectName);
        }

        public async Task<WorkItemResponse> GetDetailAsync(int workItemId)
        {
            WorkItemResponse wiR = new WorkItemResponse();
            string url = string.Format("{0}{1}?api-version=5.1", wipatch, workItemId);

            using (HttpClient client = new HttpClient())
            {
                Dictionary<string, string> p = new Dictionary<string, string>();

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(token)));

                using (HttpResponseMessage response = client.GetAsync(url).Result)
                {
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    wiR = JsonSerializer.Deserialize<WorkItemResponse>(responseBody);
                }
            }

            return wiR;
        }

        public async Task<List<WorkItemResponse>> GetChildsAsync(int workItemId)
        {
            List<WorkItemResponse> lstChilds = new List<WorkItemResponse>();
            WorkItemResponse wiR = new WorkItemResponse();
            string url = string.Format("{0}{1}?$expand=Relations&api-version=5.1", wipatch, workItemId);

            using (HttpClient client = new HttpClient())
            {
                Dictionary<string, string> p = new Dictionary<string, string>();

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(token)));

                using (HttpResponseMessage response = client.GetAsync(url).Result)
                {
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    wiR = JsonSerializer.Deserialize<WorkItemResponse>(responseBody);

                    List<int> lstRelChilds = wiR.relations.Where(x => x.attributes.name == "Child").Select(x => int.Parse(x.url.Substring(x.url.LastIndexOf("/") + 1))).ToList();
                    foreach (int c in lstRelChilds)
                    {
                        WorkItemResponse child = await GetDetailAsync(c);
                        lstChilds.Add(child);
                    }
                }
            }

            return lstChilds;
        }

        public async Task UpdateToDevReviewAsync(string workItemId)
        {
            string url = string.Format("{0}{1}?api-version=5.1", wipatch, workItemId);

            //Update WI
            using (HttpClient client = new HttpClient())
            {
                Dictionary<string, string> p = new Dictionary<string, string>();

                string jsonRequest = "[{\"op\": \"add\", \"path\": \"/fields/System.State\", \"value\": \"Waiting QA Deployment\"}]";
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json-patch+json");

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(token)));

                using (HttpResponseMessage response = client.PatchAsync(url, content).Result)
                {
                    response.EnsureSuccessStatusCode();
                }
            }
        }

        public async Task UpdateToQAAsync(string workItemId)
        {
            string url = string.Format("{0}{1}?api-version=5.1", wipatch, workItemId);

            //Update WI
            using (HttpClient client = new HttpClient())
            {
                Dictionary<string, string> p = new Dictionary<string, string>();

                string jsonRequest = "[{\"op\": \"add\", \"path\": \"/fields/System.State\", \"value\": \"QA\"}]";
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json-patch+json");

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(token)));

                using (HttpResponseMessage response = client.PatchAsync(url, content).Result)
                {
                    response.EnsureSuccessStatusCode();
                }
            }
        }

        public async Task<List<string>> GetPullRequestAsync(int workItemId)
        {
            WorkItemResponse wiR = new WorkItemResponse();
            PullRequestResponse prR = new PullRequestResponse();
            string url = string.Format("{0}{1}?api-version=6.0&$expand=relations", wipatch, workItemId);
            List<string> resp = null;
            List<string> respFilter = new List<string>();

            using (HttpClient client = new HttpClient())
            {
                Dictionary<string, string> p = new Dictionary<string, string>();

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(token)));

                using (HttpResponseMessage response = client.GetAsync(url).Result)
                {
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    wiR = JsonSerializer.Deserialize<WorkItemResponse>(responseBody);

                    resp = (from w in wiR.relations
                            where w.url.Contains("Git/PullRequestId/")
                            select w.url.Split("%2F")[2]).ToList();

                    //###
                    foreach (string pr in resp)
                    {
                        url = string.Format("{0}{1}?api-version=6.0", prurl, pr);

                        using (HttpResponseMessage response2 = client.GetAsync(url).Result)
                        {
                            response.EnsureSuccessStatusCode();
                            string responseBody2 = await response2.Content.ReadAsStringAsync();
                            prR = JsonSerializer.Deserialize<PullRequestResponse>(responseBody2);
                            if (prR.targetRefName == "refs/heads/master" || prR.targetRefName == "refs/heads/e2e01" || prR.targetRefName == "refs/heads/qa2")
                            {
                                respFilter.Add(pr);
                            }
                        }
                    }


                }
            }

            return respFilter;
        }
    }
}
