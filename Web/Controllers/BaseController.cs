using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Utils;

namespace Web.Controllers
{
    public class BaseController : Controller
    {
        protected string ConnectionStringIdentity { get; }
        protected string ConnectionStringEntity { get; }

        public BaseController()
        {
            ConnectionStringIdentity = ConfigurationManager.ConnectionStrings["IdentityContext"].ToString();
            ConnectionStringEntity = ConfigurationManager.ConnectionStrings["TotalDbContext"].ToString();
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            string userName = filterContext.HttpContext.User.Identity == null ? string.Empty : filterContext.HttpContext.User.Identity.Name;
            filterContext.ExceptionHandled = true;
            Log.Error(string.Format("USER:[{0}] {1}", userName, filterContext.Exception.Message), filterContext.Exception);
        }
    }
}