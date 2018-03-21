using OnlinePrint.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace OnlinePrint.Controllers
{
    public class AdminController : Controller
    {
        public AdminController()
        {
            db = new OnlinePrintDbContext();
        }
        private OnlinePrintDbContext db;

        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Index(string userName, string Password)
        {
            if (userName == "" || Password == "")
            {
                ModelState.AddModelError("", "请输入完整信息再登录！");
                return View();
            }
            else
            {
                if (ValidateUser(userName, Password))
                {
                    FormsAuthentication.SetAuthCookie(userName, false);
                    return RedirectToAction("AdminLogined");
                }
                else
                {
                    //提示行而非弹窗形式
                    ModelState.AddModelError("", "您输入的账号或者密码有误！");
                    return View();
                }
            }
        }

        //验证函数
        public bool ValidateUser(string Account, string Password)
        {
            var member = (from p in db.Administrator
                          where p.AdminID == Account && p.Password == Password
                          select p).FirstOrDefault();
            if (member != null)
            {
                Session["AdminID"] = member.AdminID;
                return true;
            }
            return false;
        }

        //verify model
        public ActionResult Verification(int index = 1)
        {
            if (Session["AdminID"] != null)
            {
                ViewBag.AdminLogined = true;

                var models = db.ModelLibrary.Where(n => n.ModelState == false);
                Paging<ModelLibrary> modelPaging = new Paging<ModelLibrary>(4, models);
                modelPaging.PageIndex = index;
                return View(modelPaging);
            }
            else
            {
                return RedirectToAction("Index");
            }
            
        }

        public ActionResult VerificationModel(string modelId)
        {
            if (Session["AdminID"] != null)
            {
                ViewBag.AdminLogined = true;
                //var model = db.ModelLibrary.Where(n => n.ModelID == modelId).FirstOrDefault();
                //var usermodel = db.UsersModels.Where(n => n.ModelID == modelId).FirstOrDefault();
                //var user = db.Users.Where(n => n.UserID == usermodel.UserID).FirstOrDefault();
                var user = from q in db.Users
                           join r in db.UsersModels on q.UserID equals r.UserID
                           where r.ModelID == modelId
                           select q;
                ViewBag.UserID = user.FirstOrDefault().UserID;
                ViewBag.UserName = user.FirstOrDefault().UserName;
                ModelDetails modelDetails = new ModelDetails();
                var model =  db.ModelLibrary.Where(n => n.ModelID == modelId).FirstOrDefault();
                var modelPartDetails = db.ModelPartsDetail.Where(n => n.ModelID == modelId);
                modelDetails.Model = model;
                foreach(var item in modelPartDetails)
                {
                    modelDetails.ModelDetail.Add(item);
                }
                return View(modelDetails);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        public ActionResult PassVerification(FormCollection form)
        {
            if (Session["AdminID"] != null)
            {
                ViewBag.AdminLogined = true;

                string userName = form["userName"];
                string modelId = form["modelId"];
                decimal printPrice = Convert.ToDecimal(form["printPrice"]);
                int downloadTickets = Convert.ToInt32(form["downloadTickets"]);
                string newTags = form["modelTags"];
                string[] Tags = newTags.Split(' ');
                var user = db.Users.Where(n => n.UserName == userName).FirstOrDefault();
                user.UserDownloadTickets += downloadTickets;
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();

                var model = db.ModelLibrary.Where(n => n.ModelID == modelId).FirstOrDefault();
                foreach (var tag in Tags)
                {
                    model.ModelKeyWords +=  tag + ",";
                }
                model.ModelState = true;
                model.ModelPrintPrice = printPrice;
                db.Entry(model).State = EntityState.Modified;
                db.SaveChanges();

                //modify part model's print price
                var priceString = form["partPrintPrice"];
                var filesNameString = form["fileName"];
                if(priceString != null && filesNameString != null)
                {
                    var fileNames = filesNameString.Split(',');
                    var prices = priceString.Split(',');
                    var modelDetails = db.ModelPartsDetail.Where(n => n.ModelID == modelId);
                    for(int i = 0; i < fileNames.Count(); i++)
                    {
                        foreach(var item in modelDetails)
                        {
                            if(item.ModelFileName == fileNames[i])
                            {
                                item.ModelPartPrintPrice = Convert.ToDecimal(prices[i]);
                                
                                break;
                            }
                        }
                    }
                    db.SaveChanges();
                }
                return RedirectToAction("Verification");
            }
            else
            {
                return RedirectToAction("Index");
            }
        }
        public ActionResult DeleteModel(string modelId)
        {
            if (Session["AdminID"] != null)
            {
                ViewBag.AdminLogined = true;

                var userquery = db.ModelLibrary.First(n => n.ModelID == modelId);
                db.ModelLibrary.Remove(userquery);
                db.SaveChanges();
                return RedirectToAction("Verification");
            }
            else
            {
                return RedirectToAction("Index");

            }
        }

        public ActionResult Information()
        {
            if (Session["AdminID"] != null)
            {
                ViewBag.AdminLogined = true;

                var Account = Session["AdminID"].ToString();
                var admins = db.Administrator.Where(n => n.AdminID == Account);
                return View(admins);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        //orders
        public ActionResult Orders(int index = 1)
        {
            if (Session["AdminID"] != null)
            {
                ViewBag.AdminLogined = true;

                var orders = db.Orders.Where(n => (n.State != (Int32)OrderState.Deleted) || 
                                                   (n.State != (Int32)OrderState.Canceled)).
                                                    OrderByDescending(n => n.OrderID);
                Paging<Orders> userModelsPage = new Paging<Orders>(6, orders);
                userModelsPage.PageIndex = index; 
                return View(userModelsPage);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }
        //printer resource
        public ActionResult Printer()
        {
            if (Session["AdminID"] != null)
            {
                ViewBag.AdminLogined = true;

                return View();
            }
            else
            {
                return RedirectToAction("Index");
            }
        }
        public ActionResult AdminLogined()
        {
            if (Session["AdminID"] != null)
            {
                ViewBag.AdminLogined = true;

                string adminId = Session["AdminID"].ToString();
                var admin = db.Administrator.Find(adminId);
                ViewBag.Information = "欢迎你 管理员admin";
                ViewBag.AdminLogined = true;
                return View();
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        public ActionResult LoginOut()
        {
            if (Session["AdminID"] != null)
            {
                Session["AdminID"] = null;
                return RedirectToAction("Index");
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        public ActionResult OnlineOffer(string orderId)
        {
            if (Session["AdminID"] != null)
            {
                ViewBag.AdminLogined = true;

                var orderDetails = db.OrderDetail.Where(n => (n.OrderID == orderId) && 
                                                       (n.State == (Int32)OrderState.WaitPrice));
                ViewBag.OrderID = orderDetails.FirstOrDefault().OrderID;
                return View(orderDetails);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        public ActionResult InformUser(string orderId)
        {
            if (Session["AdminID"] != null)
            {
                ViewBag.AdminLogined = true;

                Orders orders = db.Orders.Where(n => n.OrderID == orderId).FirstOrDefault();
                UserMessage userMessage = new UserMessage();
                //messageID
                Random ran = new Random();
                var randomID = ran.Next(10, 99);
                var messageID = DateTime.Now.ToString("yyyyMMddHHmmss") + randomID.ToString();
                userMessage.MessageID = messageID;
                userMessage.UserID = orders.UserID;
                userMessage.MessageSendTime = DateTime.Now;
                userMessage.MessageContent = "订单编号:" + orders.OrderID + " " + "订单总价:" + orders.Sums + " " + "订单状态:已报价";
                userMessage.State = false;
                db.UserMessage.Add(userMessage);
                db.SaveChanges();
                return RedirectToAction("Orders");
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public ActionResult Calculate(FormCollection form)
        {
            if (Session["AdminID"] != null)
            {
                ViewBag.AdminLogined = true;

                string printTimeStrings = form["printTimeConsuming"];
                string printConsumptionStrings = form["printConsumption"];
                string orderParamStrings = form["parameter"].ToString();
                string modelIdStrings = form["modelId"].ToString();
                string orderId = form["orderId"].ToString();

                string[] printTimes = printTimeStrings.Split(new char[] { ',' });
                string[] printConsumptions = printConsumptionStrings.Split(new char[] { ',' });
                string[] orderParams = orderParamStrings.Split(new char[] { ',' });
                string[] modelIds = modelIdStrings.Split(new char[] { ',' });
                int count = modelIds.Count();
                Orders order = db.Orders.Find(orderId);
                for (int i = 0; i < count; i++)
                {
                    double printTime = Convert.ToDouble(printTimes[i]);
                    double printConsumption = Convert.ToDouble(printConsumptions[i]);
                    string[] parameters = orderParams[i].Split(new char[] { ';', ':' });

                    int materialRatio = 1;
                    switch (parameters[1])
                    {
                        case "PLA塑料":
                            materialRatio = 1;
                            break;

                        case "ABS塑料":
                            materialRatio = 1;
                            break;

                        case "光敏树脂":
                            materialRatio = 1;
                            break;
                    }
                    int materialColorRatio = 1;
                    int postProcessingRatio = 1;
                    switch (parameters[9])
                    {
                        case "无需打磨":
                            postProcessingRatio = 0;
                            break;
                        case "打磨内表面":
                            postProcessingRatio = 5;
                            break;
                        case "打磨外表面":
                            postProcessingRatio = 5;
                            break;
                        case "打磨内外表面":
                            postProcessingRatio = 5;
                            break;
                    }
                    double singleModelPrice = 2 * ((0.7 * printTime + 0.2 * printConsumption * materialRatio * materialColorRatio) + postProcessingRatio);
                    string modelId = modelIds[i];
                    string orderParam = orderParams[i];
                    OrderDetail orderDetail = db.OrderDetail.Where(n => (n.ModelID == modelId) && (n.PrintParameter == orderParam) && (n.OrderID == orderId)).FirstOrDefault();
                    orderDetail.Price = Convert.ToDecimal(singleModelPrice);
                    orderDetail.Sums = orderDetail.Price * orderDetail.Counts;
                    orderDetail.State = (Int32)OrderState.WaitConfirm;
                    db.Entry(orderDetail).State = EntityState.Modified;
                    db.SaveChanges();
                    order.Sums += + orderDetail.Sums;
                }
                order.State = (Int32)OrderState.WaitConfirm;
                db.Entry(order).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Orders");
            }
            else
            {
                return RedirectToAction("Index");
            }


            //old version
            //double printTime = Convert.ToDouble(form["printTimeConsuming"]);
            //double printConsumption = Convert.ToDouble(form["printConsumption"]);
            //var modelParam = form["parameter"].ToString();
            //var modelId = form["modelId"].ToString();
            //var orderId = form["orderId"].ToString();
            //string[] s1 = modelParam.Split(new char[] { ' ' });
            //int materialRatio = 1;
            //switch (s1[0])
            //{
            //    case "材料:PLA塑料":
            //        materialRatio = 1;
            //        break;

            //    case "材料:ABS塑料":
            //        materialRatio = 1;
            //        break;

            //    case "材料:光敏树脂":
            //        materialRatio = 1;
            //        break;
            //}
            //int materialColorRatio = 1;
            //int postProcessingRatio = 1;
            //switch (s1[4])
            //{
            //    case "打磨选项:无需打磨":
            //        postProcessingRatio = 0;
            //        break;
            //    case "打磨选项:打磨内表面":
            //        postProcessingRatio = 5;
            //        break;
            //    case "打磨选项:打磨外表面":
            //        postProcessingRatio = 5;
            //        break;
            //    case "打磨选项:打磨内外表面":
            //        postProcessingRatio = 5;
            //        break;
            //}
            //double singleModelPrice = 2 * ((0.7 * printTime + 0.2 * printConsumption * materialRatio * materialColorRatio) + postProcessingRatio);
            //OrderDetail model = db.OrderDetail.Where(n => (n.ModelID == modelId) && (n.PrintParameter == modelParam) && (n.OrderID == orderId)).FirstOrDefault();
            //model.Price = Convert.ToDecimal(singleModelPrice);
            //model.Sums = model.Price * model.Counts;
            //db.Entry(model).State = EntityState.Modified;
            //db.SaveChanges();

            //Orders order = db.Orders.Find(orderId);
            //order.Sums = order.Sums + model.Sums;
            //db.Entry(order).State = EntityState.Modified;
            //db.SaveChanges();
            //var orders = db.OrderDetail.Where(n => n.OrderID == orderId);
            //return RedirectToAction("OnlineOffer","Admin",new { orderId = orderId });

        }
    }
}