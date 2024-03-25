namespace RS.api.Models
{
    public class PullRequestParam
    {
        public Resource resource { get; set; }
    }

    public class Resource
    {
        public int pullRequestId { get; set; }

        public string status { get; set; }
    }

    public class PullRequestParamExt
    {
        public Resource pullRequest { get; set; }
    }
}
