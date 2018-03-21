using OnlinePrint.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OnlinePrint.Controllers
{
    public class HomeController : Controller
    {
        private OnlinePrintDbContext db;

        public HomeController()
        {
            db = new OnlinePrintDbContext();
        }
        public ActionResult Index()
        {
            if (Session["UserID"] != null)
            {
                ViewBag.UserId = Session["UserID"];
                int userId = Convert.ToInt32(Session["UserID"].ToString());
                var user = db.Users.Where(n => n.UserID == userId).FirstOrDefault();
                ViewBag.UserName = user.UserName;
            }
            return View();
        }

        public ActionResult AboutUs()
        {
            if (Session["UserID"] != null)
            {
                ViewBag.UserId = Session["UserID"];
                int userId = Convert.ToInt32(Session["UserID"].ToString());
                var user = db.Users.Where(n => n.UserID == userId).FirstOrDefault();
                ViewBag.UserName = user.UserName;
            }
           
            return View();
        }

        public ActionResult Customize()
        {
            if (Session["UserID"] != null)
            {
                ViewBag.UserId = Session["UserID"];
                int userId = Convert.ToInt32(Session["UserID"].ToString());
                var user = db.Users.Where(n => n.UserID == userId).FirstOrDefault();
                ViewBag.UserName = user.UserName;
            }
            return View();
        }
        public ActionResult DownloadTickets()
        {
            if (Session["UserID"] != null)
            {
                ViewBag.UserId = Session["UserID"];
                int userId = Convert.ToInt32(Session["UserID"].ToString());
                var user = db.Users.Where(n => n.UserID == userId).FirstOrDefault();
                ViewBag.UserName = user.UserName;
            }
            return View();
        }
        public ActionResult PrintProcess()
        {
            if (Session["UserID"] != null)
            {
                ViewBag.UserId = Session["UserID"];
                int userId = Convert.ToInt32(Session["UserID"].ToString());
                var user = db.Users.Where(n => n.UserID == userId).FirstOrDefault();
                ViewBag.UserName = user.UserName;
            }
            return View();
        }
        public ActionResult Refund()
        {
            if (Session["UserID"] != null)
            {
                ViewBag.UserId = Session["UserID"];
                int userId = Convert.ToInt32(Session["UserID"].ToString());
                var user = db.Users.Where(n => n.UserID == userId).FirstOrDefault();
                ViewBag.UserName = user.UserName;
            }
            return View();
        }
        public ActionResult ReturnOfGoods()
        {
            if (Session["UserID"] != null)
            {
                ViewBag.UserId = Session["UserID"];
                int userId = Convert.ToInt32(Session["UserID"].ToString());
                var user = db.Users.Where(n => n.UserID == userId).FirstOrDefault();
                ViewBag.UserName = user.UserName;
            }
            return View();
        }
        public ActionResult RulesOfTickets()
        {
            if (Session["UserID"] != null)
            {
                ViewBag.UserId = Session["UserID"];
                int userId = Convert.ToInt32(Session["UserID"].ToString());
                var user = db.Users.Where(n => n.UserID == userId).FirstOrDefault();
                ViewBag.UserName = user.UserName;
            }
            return View();
        }
        public ActionResult SetPrintParameter()
        {
            if (Session["UserID"] != null)
            {
                ViewBag.UserId = Session["UserID"];
                int userId = Convert.ToInt32(Session["UserID"].ToString());
                var user = db.Users.Where(n => n.UserID == userId).FirstOrDefault();
                ViewBag.UserName = user.UserName;
            }
            return View();
        }
    }
}