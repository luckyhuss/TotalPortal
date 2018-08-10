using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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
    }
}