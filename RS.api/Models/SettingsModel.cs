using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RS.api.Models
{
    public class SettingsModel
    {
        private readonly IConfiguration configuration;

        public SettingsModel(IConfiguration configuration)
        {
            this.configuration = configuration;
            this.AllowedReferer = configuration.GetSection("AppSettings").GetSection("AllowedReferer").Value.Split(",").ToList<string>();
            this.Debug = bool.Parse(configuration.GetSection("AppSettings").GetSection("DebugMode").Value);
            this.ReleaseDate = DateTime.Parse(configuration.GetSection("AppSettings").GetSection("ReleaseDate").Value);
        }

        public List<string> AllowedReferer { get; set; }
        public bool Debug { get; set; }
        public DateTime ReleaseDate { get; set; }

    }
}
