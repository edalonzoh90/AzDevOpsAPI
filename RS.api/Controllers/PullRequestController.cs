using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RS.api.Models;
using RS.api.Services;
using System;
using System.Threading.Tasks;

namespace RS.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PullRequestController : ControllerBase
    {
        private readonly IPullRequestService _pullRequestService;
        private readonly IPullRequestStatusesService _pullRequestStatusesService;
        private readonly IWorkItemService _workItemService;

        public PullRequestController(
            IPullRequestService pullRequestService,
            IWorkItemService workItemService,
            IPullRequestStatusesService pullRequestStatusesService
            )
        {
            _pullRequestService = pullRequestService;
            _workItemService = workItemService;
            _pullRequestStatusesService = pullRequestStatusesService;
        }

        //OK
        [Route("CompleteWorkItems")]
        [HttpPost]
        public async Task<IActionResult> CompleteItems(PullRequestParam param)
        {
            string resp = "{'status': 0}";
            if (param.resource.status != "completed")
                return new JsonResult(resp);

            PullRequestResponse pr = await _pullRequestService.GetWorkItems(param);
            foreach (WorkItem wi in pr.value)
            {
                WorkItemResponse wiR = await _workItemService.GetDetailAsync(int.Parse(wi.id));

                if ((wiR.fields.SystemWorkItemType == "Bug" || wiR.fields.SystemWorkItemType == "Task")
                    && (wiR.fields.SystemState == "Dev Review"))
                {
                    await _workItemService.UpdateToDevReviewAsync(wi.id);
                }
            }

            return new JsonResult(resp);
        }

        //[Route("CompleteDBWorkItems")]
        //[HttpPost]
        //public async Task<IActionResult> CompleteDBItems(PullRequestParam param)
        //{
        //    string resp = "{'status': 0}";
        //    if (param.resource.status != "completed")
        //        return new JsonResult(resp);

        //    PullRequestResponse pr = await _pullRequestService.GetBDWorkItems(param);
        //    foreach (WorkItem wi in pr.value)
        //    {
        //        WorkItemResponse wiR = await _workItemService.GetDetailAsync(int.Parse(wi.id));

        //        if ((wiR.fields.SystemWorkItemType == "User Story")
        //            && (wiR.fields.SystemState == "New"))
        //        {
        //            await _workItemService.UpdateToQAAsync(wi.id);
        //        }
        //    }

        //    return new JsonResult(resp);
        //}

        //OK
        [Route("ValidateWorkItems")]
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ValidateWorkItems(PullRequestParam param)
        {
            string resp = "{'status': 0}";

            try
            {
                if (param.resource.status == "completed")
                    return new JsonResult(resp);

                bool wiStatus = await _pullRequestService.ValidateWorkItemStatusAsync(param);

                PullRequestStatusParam prParam = new PullRequestStatusParam()
                {
                    context = new ContextModel() { name = "Work Item State" },
                    state = wiStatus ? PullRequestStatusParam.SUCCESS_STATE : PullRequestStatusParam.FAIL_STATE
                };

                await _pullRequestStatusesService.SetStatusAsync(param.resource.pullRequestId, prParam);
            }
            catch (Exception ex)
            {
                return new JsonResult(ex);
            }

            return new JsonResult(resp);
        }

        [Route("ValidateTargetDate")]
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ValidateTargetDate(PullRequestParam param)
        {
            string resp = "{'status': 0}";

            try
            {
                if (param.resource.status == "completed")
                    return new JsonResult(resp);

                bool wiStatus = await _pullRequestService.ValidateWorkItemTargetDateAsync(param);

                PullRequestStatusParam prParam = new PullRequestStatusParam()
                {
                    context = new ContextModel() { name = "Work Item State" },
                    state = wiStatus ? PullRequestStatusParam.SUCCESS_STATE : PullRequestStatusParam.FAIL_STATE
                };

                //await _pullRequestStatusesService.SetStatusAsync(param.resource.pullRequestId, prParam);
            }
            catch (Exception ex)
            {
                return new JsonResult(ex);
            }

            return new JsonResult(resp);
        }
    }
}

