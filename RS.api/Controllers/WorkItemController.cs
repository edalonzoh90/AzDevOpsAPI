using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RS.api.Models;
using RS.api.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RS.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkItemController : ControllerBase
    {
        private readonly IPullRequestService _pullRequestService;
        private readonly IPullRequestStatusesService _pullRequestStatusesService;
        private readonly IWorkItemService _workItemService;

        public WorkItemController(
            IPullRequestStatusesService pullRequestStatusesService,
            IPullRequestService pullRequestService,
            IWorkItemService workItemService)
        {
            _pullRequestStatusesService = pullRequestStatusesService;
            _pullRequestService = pullRequestService;
            _workItemService = workItemService;
        }
        //OK
        [Route("UserStory/ValidateUpdate")]
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ValidateUpdateUS(WorkItemParam wiParam)
        {
            string resp = "{ 'status': 0}";
            //Verify if is Task or Bug -> This validation comes from the hook

            //Verify the state of the task
            if (wiParam.resource.fields.SystemState.newValue != "QA")
                return new JsonResult(resp);

            if (wiParam.resource.revision.fields.SystemParent == 0)
                return new JsonResult(resp);

            //Get the Parent 
            WorkItemResponse wiParent = await _workItemService.GetDetailAsync(wiParam.resource.revision.fields.SystemParent);

            //Verify if is US
            if (wiParent.fields.SystemWorkItemType == "User Story"
                && (wiParent.fields.SystemState != "QA" && wiParent.fields.SystemState != "QA In-Progress"))
            {
                string[] AceptedTaskStates = { "Removed", "Closed", "Resolved", "QA Passed", "QA", "Waiting QA Deployment" };
                string[] AceptedBugStates = { "Closed", "QA Passed", "QA", "Resolved", "Waiting QA Deployment" };
                bool changeUserStoryToQA = true;

                //Get all its childs and verify its status
                List<WorkItemResponse> lstChilds = await _workItemService.GetChildsAsync(wiParam.resource.revision.fields.SystemParent);
                foreach (WorkItemResponse child in lstChilds)
                {
                    //If all are in QA and the US is not QA -> update the status to QA 
                    if (
                        (child.fields.SystemWorkItemType == "Bug" && !AceptedBugStates.Contains(child.fields.SystemState))
                        ||
                        (child.fields.SystemWorkItemType == "Task" && !AceptedTaskStates.Contains(child.fields.SystemState))
                        )
                    {
                        changeUserStoryToQA = false;
                        break;
                    }
                }

                if (changeUserStoryToQA)
                {
                    await _workItemService.UpdateToQAAsync(wiParam.resource.revision.fields.SystemParent.ToString());
                }
            }

            return new JsonResult(resp);
        }

        [Route("{WorkItem}/PullRequests")]
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> PullRequest(int workitem)
        {

            var resp = await _workItemService.GetPullRequestAsync(workitem);

            return new JsonResult(resp);
        }


    }
}
