using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RS.api.Models
{
    public class WorkItemParam
    {
        public WIResource resource { get; set; }
    }

    public class SystemState
    {
        public string oldValue { get; set; }
        public string newValue { get; set; }
    }

    public class WIFields
    {
        [JsonPropertyName("System.State")]
        public SystemState SystemState { get; set; }

        [JsonPropertyName("System.Parent")]
        public int SystemParent { get; set; }
    }

    public class Relation
    {
        public string rel { get; set; }
        public string url { get; set; }
        public Attributes attributes { get; set; }
    }

    public class WIResource
    {
        public int workItemId { get; set; }
        public Revision revision { get; set; }
        public string url { get; set; }
        public WIFields fields { get; set; }
    }

    public class RevFields
    {

        [JsonPropertyName("System.Parent")]
        public int SystemParent { get; set; }
    }

    public class Revision
    {
        public List<Relation> relations { get; set; }
        public RevFields fields { get; set; }
    }
}
