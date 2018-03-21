using OnlinePrint.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace OnlinePrint.Controllers
{
    public class OrderController : Controller
    {
        public OrderController()
        {
           db  = new OnlinePrintDbContext();
        }

        private OnlinePrintDbContext db;

        public ActionResult Index()
        {
            return View();
        }

        //parameter
        public ActionResult Parameter()
        {
            return View();
        }
      
       
      
    }
}