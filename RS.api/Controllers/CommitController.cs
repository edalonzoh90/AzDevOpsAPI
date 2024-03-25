using Microsoft.AspNetCore.Mvc;
using RS.data.Filters;
using RS.data.Model;
using RS.data.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RS.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommitController : ControllerBase
    {
        private readonly ICommitService _commitService;

        public CommitController(
            ICommitService commitService
            )
        {
            _commitService = commitService;
        }

        [HttpPost]
        public async Task<IActionResult> Post(List<Commit> param)
        {
            string resp = "{}";

            try
            {
                _commitService.InsertComplete(param);
            }
            catch (Exception ex)
            {
                return new JsonResult(ex);
            }

            return new JsonResult(resp);
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            CommitFilter param = new CommitFilter();
            IEnumerable resp = null;
            try
            {
                resp = _commitService.List(param);
            }
            catch (Exception ex)
            {
                return new JsonResult(ex);
            }

            return new JsonResult(resp);
        }

        [Route("workitem/{WorkItem}")]
        [HttpGet]
        public async Task<IActionResult> GetByWorkItem(int WorkItem)
        {
            CommitFilter param = new CommitFilter();
            param.WorkItemId = WorkItem;

            IEnumerable resp = null;
            try
            {
                resp = _commitService.List(param);
            }
            catch (Exception ex)
            {
                return new JsonResult(ex);
            }

            return new JsonResult(resp);
        }
    }
}
