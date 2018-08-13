using log4net;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Utils;

namespace Web.Helpers
{
    /// <summary>
    /// Logs the ActionExecuting and ActionExecuted when the 'IsDebugEnabled' is enabled. The 'ActionExecuted' handler calculates the time taken in milliseconds for the request to complete.
    /// </summary>
    public class LoginFilterAttribute : ActionFilterAttribute
    {
        private const string StopwatchKey = "DebugLoggingStopWatch";

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (Log.IsDebugEnabled)
            {
                var loggingWatch = Stopwatch.StartNew();
                filterContext.HttpContext.Items.Add(StopwatchKey, loggingWatch);

                var message = new StringBuilder();
                message.Append(string.Format("Executing controller {0}, action {1}",
                    filterContext.ActionDescriptor.ControllerDescriptor.ControllerName,
                    filterContext.ActionDescriptor.ActionName));

                Log.Debug(message.ToString());
            }
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (Log.IsDebugEnabled)
            {
                if (filterContext.HttpContext.Items[StopwatchKey] != null)
                {
                    var loggingWatch = (Stopwatch)filterContext.HttpContext.Items[StopwatchKey];
                    loggingWatch.Stop();

                    long timeSpent = loggingWatch.ElapsedMilliseconds;

                    var message = new StringBuilder();
                    message.Append(string.Format("Finished executing controller {0}, action {1} - time spent {2}",
                        filterContext.ActionDescriptor.ControllerDescriptor.ControllerName,
                    filterContext.ActionDescriptor.ActionName,
                    timeSpent));

                    Log.Debug(message.ToString());
                    filterContext.HttpContext.Items.Remove(StopwatchKey);
                }
            }
        }
    }
}