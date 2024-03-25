using log4net;
using log4net.Config;
using log4net.Repository;
using Log4NetSample.LogUtility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using RS.api.Models;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace RS.api.Filters
{
    public class ActionCollectorFilter : IActionFilter
    {
        private readonly IConfiguration configuration;
        ILoggerRepository logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
        Logger logger;
        private SettingsModel settings;

        public ActionCollectorFilter(IConfiguration configuration)
        {
            settings = new SettingsModel(configuration);
            XmlConfigurator.Configure(logRepository, new FileInfo("log4netconfig.config"));
            logger = new Logger();
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            string Headers = ParseKeys(context.HttpContext.Request.Headers);
            //logger.Info(Headers);

            if (!settings.Debug)
            {
                bool allow = false;
                foreach (var find in settings.AllowedReferer)
                {
                    string referer = context.HttpContext.Request.Headers["Referer"];
                    if (referer != null && referer.Contains(find))
                    {
                        allow = true;
                        break;
                    }
                }

                if (!allow)
                    context.Result = new StatusCodeResult(StatusCodes.Status404NotFound);
            }
        }

        private static string ParseKeys(IEnumerable<KeyValuePair<string, StringValues>> values)
        {
            var sb = new StringBuilder();
            foreach (var value in values)
            {
                sb.AppendLine(value.Key + " = " + string.Join(", ", value.Value));
            }
            return sb.ToString();
        }

    }
}
