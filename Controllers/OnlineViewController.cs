using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OnlinePrint.Controllers
{
    public class OnlineViewController : Controller
    {
        // GET: OnlineView
        public ActionResult Index()
        {
            return View();
        }
    }
}