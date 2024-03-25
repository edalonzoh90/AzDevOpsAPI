using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RS.api.Models
{
    public class WorkItemResponse
    {
        public WorkItemResponse()
        {
            fields = new Fields();
        }

        public Fields fields { get; set; }

        public List<Relation> relations { get; set; }
    }

    public class Fields
    {
        [JsonPropertyName("System.WorkItemType")]
        public string SystemWorkItemType { get; set; }

        [JsonPropertyName("System.State")]
        public string SystemState { get; set; }

        //[System.Text.Json.Serialization.JsonConverter(typeof(CustomDateTimeConverter))]
        [JsonPropertyName("Microsoft.VSTS.Scheduling.TargetDate")]
        public DateTime TargetDate { get; set; }

    }

    public class Attributes
    {
        public DateTime authorizedDate { get; set; }
        public int id { get; set; }
        public DateTime resourceCreatedDate { get; set; }
        public DateTime resourceModifiedDate { get; set; }
        public DateTime revisedDate { get; set; }
        public string name { get; set; }
    }
}
