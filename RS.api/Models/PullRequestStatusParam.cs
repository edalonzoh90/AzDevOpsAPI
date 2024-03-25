namespace RS.api.Models
{
    public class PullRequestStatusParam
    {
        public static string SUCCESS_STATE = "succeeded";
        public static string FAIL_STATE = "failed";

        public PullRequestStatusParam()
        {
            context = new ContextModel();
        }

        public string state { get; set; }

        public string description { get; set; }

        public string targetUrl { get; set; }

        public ContextModel context { get; set; }
    }

    public class ContextModel
    {
        public string name { get; set; }
        public string genre { get; set; }
    }

}
