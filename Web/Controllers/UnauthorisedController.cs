using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Web.Controllers
{
    public class UnauthorisedController : Controller
    {
        // GET: Unauthorized
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Error(string _errorMsg)
        {
            ViewBag.ErrorMsg = _errorMsg;
            return View();
        }
    }
}