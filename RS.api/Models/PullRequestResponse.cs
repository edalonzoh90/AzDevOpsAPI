using System.Collections.Generic;

namespace RS.api.Models
{
    public class PullRequestResponse
    {
        public List<WorkItem> value { get; set; }

        public string targetRefName { get; set; }

    }


    public class WorkItem
    {
        public string id { get; set; }
    }
}
