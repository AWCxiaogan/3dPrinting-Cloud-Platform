using OnlinePrint.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Data.Entity.SqlServer;
using System.Web.Mvc;
using System.Data.Entity;
using System.Diagnostics;

namespace OnlinePrint.Controllers
{
    enum QueryString
    {
        queryNull,
        queryTime,
        queryMinprice,
        queryMaxprice,
        queryOrder,
        queryTimeMinprice,
        queryTimeMaxprice,
        queryTimeOrder,
        queryOrderMinprice,
        queryOrderMaxprice,
        queryMinpriceMaxprice,
        queryTimeMinpriceMaxprice,
        queryTimeOrderMinprice,
        queryTimeOrderMaxprice,
        queryOrderMinpriceMaxprice,
        queryTimeMinpriceMaxpriceOrder,
    }
    public class ModelController : Controller
    {
        public ModelController()
        {
            SQLQuery = QueryString.queryNull;
            db = new OnlinePrintDbContext();
        }

        private QueryString SQLQuery;
        private OnlinePrintDbContext db;


        // GET: Model
        public ActionResult Index(int index = 1)
        {
            //judge if user is logined
            if (Session["UserID"] != null)
            {
                ViewBag.UserId = Session["UserID"];
                int userId = Convert.ToInt32(Session["UserID"].ToString());
                var user = db.Users.Find(userId);
                ViewBag.UserName = user.UserName;
            }
            var filterTime = Request.QueryString["filterTime"];
            var minPrice = Request.QueryString["minPrice"];
            var maxPrice = Request.QueryString["maxPrice"];
            if (minPrice == "")
            {
                minPrice = null;
            }
            if (maxPrice == "")
            {
                maxPrice = null;
            }
            var orderBy = Request.QueryString["orderBy"];
            PagingForFilter<ModelLibrary> modelPaging = FilterCategory("all", filterTime, minPrice, maxPrice, orderBy, index);
            SetFilterFormParameter(filterTime, orderBy, minPrice, maxPrice);
            return View(modelPaging);
        }
        #region
        //model category
        //Jewellery
        public ActionResult Jewellery(int index = 1)
        {
            //judge if user is logined
            if (Session["UserID"] != null)
            {
                ViewBag.UserId = Session["UserID"];
                int userId = Convert.ToInt32(Session["UserID"].ToString());
                var user = db.Users.Find(userId);
                ViewBag.UserName = user.UserName;
            }
            var filterTime = Request.QueryString["filterTime"];
            var minPrice = Request.QueryString["minPrice"];
            var maxPrice = Request.QueryString["maxPrice"];
            var orderBy = Request.QueryString["orderBy"];
            if (minPrice == "")
            {
                minPrice = null;
            }
            if (maxPrice == "")
            {
                maxPrice = null;
            }
            PagingForFilter<ModelLibrary> modelPaging = FilterCategory("珠宝类", filterTime, minPrice, maxPrice, orderBy, index);
            SetFilterFormParameter(filterTime, orderBy, minPrice, maxPrice);
            return View(modelPaging);
        }
        //Gift
        public ActionResult Gift(int index = 1)
        {
            //judge if user is logined
            if (Session["UserID"] != null)
            {
                ViewBag.UserId = Session["UserID"];
                int userId = Convert.ToInt32(Session["UserID"].ToString());
                var user = db.Users.Find(userId);
                ViewBag.UserName = user.UserName;
            }
            var filterTime = Request.QueryString["filterTime"];
            var minPrice = Request.QueryString["minPrice"];
            var maxPrice = Request.QueryString["maxPrice"];
            var orderBy = Request.QueryString["orderBy"];
            if (minPrice == "")
            {
                minPrice = null;
            }
            if (maxPrice == "")
            {
                maxPrice = null;
            }
            PagingForFilter<ModelLibrary> modelPaging = FilterCategory("礼品类", filterTime, minPrice, maxPrice, orderBy, index);
            SetFilterFormParameter(filterTime, orderBy, minPrice, maxPrice);
            return View(modelPaging);
        }
        //Architecture
        public ActionResult Architecture(int index = 1)
        {
            //judge if user is logined
            if (Session["UserID"] != null)
            {
                ViewBag.UserId = Session["UserID"];
                int userId = Convert.ToInt32(Session["UserID"].ToString());
                var user = db.Users.Find(userId);
                ViewBag.UserName = user.UserName;
            }
            var filterTime = Request.QueryString["filterTime"];
            var minPrice = Request.QueryString["minPrice"];
            var maxPrice = Request.QueryString["maxPrice"];
            var orderBy = Request.QueryString["orderBy"];
            if (minPrice == "")
            {
                minPrice = null;
            }
            if (maxPrice == "")
            {
                maxPrice = null;
            }
            PagingForFilter<ModelLibrary> modelPaging = FilterCategory("建筑类", filterTime, minPrice, maxPrice, orderBy, index);
            SetFilterFormParameter(filterTime, orderBy, minPrice, maxPrice);
            return View(modelPaging);
        }
        //Art
        public ActionResult Art(int index = 1)
        {
            //judge if user is logined
            if (Session["UserID"] != null)
            {
                ViewBag.UserId = Session["UserID"];
                int userId = Convert.ToInt32(Session["UserID"].ToString());
                var user = db.Users.Find(userId);
                ViewBag.UserName = user.UserName;
            }
            var filterTime = Request.QueryString["filterTime"];
            var minPrice = Request.QueryString["minPrice"];
            var maxPrice = Request.QueryString["maxPrice"];
            var orderBy = Request.QueryString["orderBy"];
            if (minPrice == "")
            {
                minPrice = null;
            }
            if (maxPrice == "")
            {
                maxPrice = null;
            }
            PagingForFilter<ModelLibrary> modelPaging = FilterCategory("艺术类", filterTime, minPrice, maxPrice, orderBy, index);
            SetFilterFormParameter(filterTime, orderBy, minPrice, maxPrice);
            return View(modelPaging);
        }
        //Comic
        public ActionResult Comic(int index = 1)
        {
            //judge if user is logined
            if (Session["UserID"] != null)
            {
                ViewBag.UserId = Session["UserID"];
                int userId = Convert.ToInt32(Session["UserID"].ToString());
                var user = db.Users.Find(userId);
                ViewBag.UserName = user.UserName;
            }
            var filterTime = Request.QueryString["filterTime"];
            var minPrice = Request.QueryString["minPrice"];
            var maxPrice = Request.QueryString["maxPrice"];
            var orderBy = Request.QueryString["orderBy"];
            if (minPrice == "")
            {
                minPrice = null;
            }
            if (maxPrice == "")
            {
                maxPrice = null;
            }
            PagingForFilter<ModelLibrary> modelPaging = FilterCategory("动漫类", filterTime, minPrice, maxPrice, orderBy, index);
            SetFilterFormParameter(filterTime, orderBy, minPrice, maxPrice);
            return View(modelPaging);
        }
        //Toy
        public ActionResult Toy(int index = 1)
        {
            //judge if user is logined
            if (Session["UserID"] != null)
            {
                ViewBag.UserId = Session["UserID"];
                int userId = Convert.ToInt32(Session["UserID"].ToString());
                var user = db.Users.Find(userId);
                ViewBag.UserName = user.UserName;
            }
            var filterTime = Request.QueryString["filterTime"];
            var minPrice = Request.QueryString["minPrice"];
            var maxPrice = Request.QueryString["maxPrice"];
            var orderBy = Request.QueryString["orderBy"];
            if (minPrice == "")
            {
                minPrice = null;
            }
            if (maxPrice == "")
            {
                maxPrice = null;
            }
            PagingForFilter<ModelLibrary> modelPaging = FilterCategory("玩具类", filterTime, minPrice, maxPrice, orderBy, index);
            SetFilterFormParameter(filterTime, orderBy, minPrice, maxPrice);
            return View(modelPaging);
        }
        //Life
        public ActionResult Life(int index = 1)
        {
            //judge if user is logined
            if (Session["UserID"] != null)
            {
                ViewBag.UserId = Session["UserID"];
                int userId = Convert.ToInt32(Session["UserID"].ToString());
                var user = db.Users.Find(userId);
                ViewBag.UserName = user.UserName;
            }
            var filterTime = Request.QueryString["filterTime"];
            var minPrice = Request.QueryString["minPrice"];
            var maxPrice = Request.QueryString["maxPrice"];
            var orderBy = Request.QueryString["orderBy"];
            if (minPrice == "")
            {
                minPrice = null;
            }
            if (maxPrice == "")
            {
                maxPrice = null;
            }
            PagingForFilter<ModelLibrary> modelPaging = FilterCategory("生活类", filterTime, minPrice, maxPrice, orderBy, index);
            SetFilterFormParameter(filterTime, orderBy, minPrice, maxPrice);
            return View(modelPaging);
        }
        //Industry
        public ActionResult Industry(int index = 1)
        {
            //judge if user is logined
            if (Session["UserID"] != null)
            {
                ViewBag.UserId = Session["UserID"];
                int userId = Convert.ToInt32(Session["UserID"].ToString());
                var user = db.Users.Find(userId);
                ViewBag.UserName = user.UserName;
            }
            var filterTime = Request.QueryString["filterTime"];
            var minPrice = Request.QueryString["minPrice"];
            var maxPrice = Request.QueryString["maxPrice"];
            var orderBy = Request.QueryString["orderBy"];
            if (minPrice == "")
            {
                minPrice = null;
            }
            if (maxPrice == "")
            {
                maxPrice = null;
            }
            PagingForFilter<ModelLibrary> modelPaging = FilterCategory("工业类", filterTime, minPrice, maxPrice, orderBy, index);
            SetFilterFormParameter(filterTime, orderBy, minPrice, maxPrice);
            return View(modelPaging);
        }
        //Portrait
        public ActionResult Portrait(int index = 1)
        {
            //judge if user is logined
            if (Session["UserID"] != null)
            {
                ViewBag.UserId = Session["UserID"];
                int userId = Convert.ToInt32(Session["UserID"].ToString());
                var user = db.Users.Find(userId);
                ViewBag.UserName = user.UserName;
            }
            var filterTime = Request.QueryString["filterTime"];
            var minPrice = Request.QueryString["minPrice"];
            var maxPrice = Request.QueryString["maxPrice"];
            var orderBy = Request.QueryString["orderBy"];
            if (minPrice == "")
            {
                minPrice = null;
            }
            if (maxPrice == "")
            {
                maxPrice = null;
            }
            PagingForFilter<ModelLibrary> modelPaging = FilterCategory("人像类", filterTime, minPrice, maxPrice, orderBy, index);
            SetFilterFormParameter(filterTime, orderBy, minPrice, maxPrice);
            return View(modelPaging);
        }
        //Customize
        public ActionResult Customize(int index = 1)
        {
            //judge if user is logined
            if (Session["UserID"] != null)
            {
                ViewBag.UserId = Session["UserID"];
                int userId = Convert.ToInt32(Session["UserID"].ToString());
                var user = db.Users.Find(userId);
                ViewBag.UserName = user.UserName;
            }
            var filterTime = Request.QueryString["filterTime"];
            var minPrice = Request.QueryString["minPrice"];
            var maxPrice = Request.QueryString["maxPrice"];
            var orderBy = Request.QueryString["orderBy"];
            if (minPrice == "")
            {
                minPrice = null;
            }
            if (maxPrice == "")
            {
                maxPrice = null;
            }
            PagingForFilter<ModelLibrary> modelPaging = FilterCategory("定制类", filterTime, minPrice, maxPrice, orderBy, index);
            SetFilterFormParameter(filterTime, orderBy, minPrice, maxPrice);
            return View(modelPaging);
        }
        #endregion
        //search result
        public ActionResult Search(int index = 1)
        {
            //judge if user is logined
            if (Session["UserID"] != null)
            {
                ViewBag.UserId = Session["UserID"];
                int userId = Convert.ToInt32(Session["UserID"].ToString());
                var user = db.Users.Find(userId);
                ViewBag.UserName = user.UserName;
            }
            var searchWords = Request.QueryString["searchWords"];
            if (searchWords == "" || searchWords == null)
            {
                return RedirectToAction("Index", "Model");
            }
            if ((searchWords.Contains(";")) || (searchWords.Contains("'")))
            {
                return Content("alert('请输入正确的搜索关键字');", "application/javascript");
            }
            
            var models = db.ModelLibrary.Where(n => n.ModelState);
            var searchedModels = new List<ModelLibrary>();
            foreach (var model in models)
            {
                if (model.ModelName.Contains(searchWords) || model.ModelKeyWords.Contains(searchWords))
                {
                    searchedModels.Add(model);
                }
            }
            PagingForSearch<ModelLibrary> modelPaging = new PagingForSearch<ModelLibrary>(1, searchedModels);
            modelPaging.PageIndex = index;
            ViewBag.SearchWord = searchWords;
            return View(modelPaging);
        }

        //model details
        public ActionResult Detail(string modelId, string partName)
        {
            //judge if user is logined
            if (Session["UserID"] != null)
            {
                ViewBag.UserId = Session["UserID"];
                int userId = Convert.ToInt32(Session["UserID"].ToString());
                var user = db.Users.Find(userId);
                ViewBag.UserName = user.UserName;
            }
     
            var model = db.ModelLibrary.Where(n => (n.ModelID == modelId)).FirstOrDefault();
            //update scan number
            model.ModelScanNumbers++;
            db.Entry(model).State = EntityState.Modified;
            db.SaveChanges();

            Detail detail = new Detail();
            //select popular model
            var popularModel = db.ModelLibrary.OrderByDescending(n => (n.ModelDownloadNumbers)).Take(3);
            foreach (var pModel in popularModel)
            {
                detail.PopularModels.Add(pModel);
            }
            if (partName == null)
            {
                //detail.DetailModelImagePath.Add(path);
                detail.DetailModel = model;
                detail.DetailModelID = modelId;
                var modelDetails = db.ModelPartsDetail.Where(n => n.ModelID == modelId);
                foreach (var item in modelDetails)
                {
                    detail.DetailModelParts.Add(item);
                }
                if (model.ModelPartCounts < 2)
                {
                    //calculate the optional ratio
                    var modelDetail = db.ModelPartsDetail.Where(n => (n.ModelID == modelId) && (n.ModelFileName == model.ModelFileNames)).FirstOrDefault();
                    for (decimal x = modelDetail.Width / 2, y = modelDetail.Length / 2, z = modelDetail.Height / 2, i = 1; x >= 30 && x <= 300 && y >= 30 && y <= 300 && z >= 20 && z <= 550; x /= 2, y /= 2, z /= 2, i += 1)
                    {
                        detail.DetailModelRatio.Add(1 / (2 * i));
                        detail.DetailModelRatio.Sort();
                    }
                    detail.DetailModelRatio.Add(1);
                    for (decimal x = modelDetail.Width * 2, y = modelDetail.Length * 2, z = modelDetail.Height * 2, i = 1; x >= 30 && x <= 300 && y >= 30 && y <= 300 && z >= 20 && z <= 550; x *= 2, y *= 2, z *= 2, i += 1)
                    {
                        detail.DetailModelRatio.Add(2 * i);
                    }
                }
                //add evaluation and trade records
                var orderDetails = db.OrderDetail.Where(n => n.ModelID == modelId).OrderByDescending(n => n.OrderID).ToList();
                for (int i = 0; i < orderDetails.Count(); i++)
                {
                    var orderId = orderDetails[i].OrderID;
                    var order = db.Orders.Where(n => n.OrderID == orderId).FirstOrDefault();
                    var user = db.Users.Where(n => n.UserID == order.UserID).FirstOrDefault();
                    if (orderDetails.ElementAt(i).Evaluation != "")
                    {
                        var evaluation = new TradeEvaluation();
                        evaluation.Content = orderDetails[i].Evaluation;
                        evaluation.EvatuationTime = DateTime.Now;
                        evaluation.Consumer = user.UserName;
                        detail.ModelTradeEvaluations.Add(evaluation);
                    }
                    var tradeRecord = new TradeRecord();
                    tradeRecord.Consumer = user.UserName;
                    tradeRecord.Sum = order.Sums;
                    tradeRecord.BuyTime = order.OrderTime;
                    detail.ModelTradeRecords.Add(tradeRecord);
                }
            }
            else
            {
                //detail.DetailModelImagePath.Add(path);
                detail.IsPart = true;
                detail.DetailModel = model;
                detail.DetailModelID = modelId;
                //calculate the optional ratio
                var modelDetail = db.ModelPartsDetail.Where(n => (n.ModelID == modelId) && (n.ModelFileName == partName)).FirstOrDefault();
                detail.DetailModelParts.Add(modelDetail);
                for (decimal x = modelDetail.Width / 2, y = modelDetail.Length / 2, z = modelDetail.Height / 2, i = 1; x >= 30 && x <= 300 && y >= 30 && y <= 300 && z >= 20 && z <= 550; x /= 2, y /= 2, z /= 2, i += 1)
                {
                    detail.DetailModelRatio.Add(1 / (2 * i));
                    detail.DetailModelRatio.Sort();
                }
                detail.DetailModelRatio.Add(1);
                for (decimal x = modelDetail.Width * 2, y = modelDetail.Length * 2, z = modelDetail.Height * 2, i = 1; x >= 30 && x <= 300 && y >= 30 && y <= 300 && z >= 20 && z <= 550; x *= 2, y *= 2, z *= 2, i += 1)
                {
                    detail.DetailModelRatio.Add(2 * i);
                }
            }
            return View(detail);
        }
        //download files
        public ActionResult DownloadFile(string modelId, string partName)
        {
            if (Session["UserID"] != null)
            {
                ViewBag.UserId = Session["UserID"];
                int userId = Convert.ToInt32(Session["UserID"].ToString());
                var user = db.Users.Find(userId);
                ViewBag.UserName = user.UserName;
                var model = db.ModelLibrary.Where(n => n.ModelID == modelId).FirstOrDefault();
                string name = "";
                string path = "";          
                var owner = from q in db.Users
                            join r in db.UsersModels on q.UserID equals r.UserID
                            where r.ModelID == modelId
                            select q;
                var modelDetail = db.ModelPartsDetail.Where(n => n.ModelID == modelId);
                if (model.ModelDownloadPrice != 0)
                {
                    bool hasDownloaded = HasDownloadFile(userId, modelId);
                    if (modelDetail.Count() < 2)
                    {
                        path = Server.MapPath(modelDetail.FirstOrDefault().ModelPath);
                        name = modelDetail.FirstOrDefault().ModelFileName;
                        if (userId != owner.FirstOrDefault().UserID)
                        {
                            bool ok = DownloadSTLFiles(path, name);
                            if (ok)
                            {
                                if (!hasDownloaded)
                                {
                                    // sub model consumer's download tickets
                                    user.UserDownloadTickets = user.UserDownloadTickets - Convert.ToInt32(model.ModelDownloadPrice);
                                    db.Entry(user).State = EntityState.Modified;
                                    db.SaveChanges();
                                    //add model owner's download tickets
                                    owner.FirstOrDefault().UserDownloadTickets += Convert.ToInt32(model.ModelDownloadPrice);
                                    db.Entry(owner).State = EntityState.Modified;
                                    db.SaveChanges();
                                    hasDownloaded = true;
                                }
                            }
                            else
                            {
                                return JavaScript("alert(\"下载失败，请重新下载！\")");
                            }
                        }
                        else
                        {
                            bool ok = DownloadSTLFiles(path, name);
                            if (!ok)
                            {
                                return JavaScript("alert(\"下载失败，请重新下载！\")");
                            }
                        }
                    }
                    else
                    {
                        //first create rar file and then download
                        path = Server.MapPath(modelDetail.FirstOrDefault().ModelPath);
                        var fileDirectory = path.Substring(0, path.LastIndexOf("\\"));
                        var rarName = model.ModelName + ".rar";
                        var rarFilePath = fileDirectory + @"\" + rarName;
                        if (!System.IO.File.Exists(rarFilePath))
                        {
                            CreateRarFile(fileDirectory, rarName);
                        }
                        if (userId != owner.FirstOrDefault().UserID)
                        {
                            bool ok = DownloadSTLFiles(rarFilePath, rarName);
                            if (ok)
                            {
                                if (!hasDownloaded)
                                {
                                    // sub model consumer's download tickets
                                    user.UserDownloadTickets = user.UserDownloadTickets - Convert.ToInt32(model.ModelDownloadPrice);
                                    db.Entry(user).State = EntityState.Modified;
                                    db.SaveChanges();
                                    //add model owner's download tickets
                                    owner.FirstOrDefault().UserDownloadTickets += Convert.ToInt32(model.ModelDownloadPrice);
                                    db.Entry(owner).State = EntityState.Modified;
                                    db.SaveChanges();
                                    hasDownloaded = true;
                                }
                            }
                            else
                            {
                                return JavaScript("alert(\"下载失败，请重新下载！\")");
                            }
                        }
                        else
                        {
                            bool ok = DownloadSTLFiles(rarFilePath, rarName);
                            if (!ok)
                            {
                                return JavaScript("alert(\"下载失败，请重新下载！\")");
                            }
                        }
                    }
                }
                else
                {
                    if (modelDetail.Count() < 2)
                    {
                        path = Server.MapPath(modelDetail.FirstOrDefault().ModelPath);
                        name = modelDetail.FirstOrDefault().ModelFileName;
                        bool ok = DownloadSTLFiles(path, name);
                        if (!ok)
                        {
                            return JavaScript("alert(\"下载失败，请重新下载！\")");
                        }
                    }
                    else
                    {
                        path = Server.MapPath(modelDetail.FirstOrDefault().ModelPath);
                        var fileDirectory = path.Substring(0, path.LastIndexOf("\\"));
                        var rarName = model.ModelName + ".rar";
                        var rarFilePath = fileDirectory + @"\" + rarName;
                        if (!System.IO.File.Exists(rarFilePath))
                        {
                            CreateRarFile(fileDirectory, rarName);
                        }
                        bool ok = DownloadSTLFiles(rarFilePath, rarName);
                        if (!ok)
                        {
                            return JavaScript("alert(\"下载失败，请重新下载！\")");
                        }
                    }
                }
                //update model's download times
                model.ModelDownloadNumbers++;
                db.Entry(model).State = EntityState.Modified;
                db.SaveChanges();
                //add download record
                AddDownloadRecord(userId, modelId);
                return Redirect(Request.UrlReferrer.ToString());
                //return File(path, "application / zip - x - compressed", name);
            }
            else
            {
                return RedirectToAction("Login", "User");
            }
        }

        //download part file of model
        public ActionResult DownloadPartFile(string modelId, string partName)
        {
            if (Session["UserID"] != null)
            {
                ViewBag.UserId = Session["UserID"];
                int userId = Convert.ToInt32(Session["UserID"].ToString());
                var user = db.Users.Find(userId);
                ViewBag.UserName = user.UserName;

                var model = db.ModelLibrary.Where(n => n.ModelID == modelId).FirstOrDefault();
                //deal with tickets
                //var ownerModel = db.UsersModels.Where(n => n.ModelID == model.ModelID).FirstOrDefault();
                // var owner = db.Users.Find(ownerModel.UserID);
                var owner = from q in db.Users
                            join r in db.UsersModels on q.UserID equals r.UserID
                            where r.ModelID == modelId
                            select q;
                var modelDetail = db.ModelPartsDetail.Where(n => (n.ModelID == modelId) && (n.ModelFileName == partName)).FirstOrDefault();
                bool hasDownloaded = HasDownloadFile(userId, modelId);
                var path = Server.MapPath(modelDetail.ModelPath);
                var name = modelDetail.ModelFileName;
                if (model.ModelDownloadPrice != 0)
                {
                    if (userId != owner.FirstOrDefault().UserID)
                    {
                        bool ok = DownloadSTLFiles(path, name);
                        if (ok)
                        {
                            if (!hasDownloaded)
                            {
                                // sub model consumer's download tickets
                                user.UserDownloadTickets = user.UserDownloadTickets - Convert.ToInt32(model.ModelDownloadPrice);
                                db.Entry(user).State = EntityState.Modified;
                                db.SaveChanges();
                                //add model owner's download tickets
                                owner.FirstOrDefault().UserDownloadTickets += Convert.ToInt32(model.ModelDownloadPrice);
                                db.Entry(owner).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                        else
                        {
                            return JavaScript("alert(\"下载失败，请重新下载！\")");
                        }
                    }
                    else
                    {
                        bool ok = DownloadSTLFiles(path, name);
                        if (!ok)
                        {
                            return JavaScript("alert(\"下载失败，请重新下载！\")");
                        }
                    }
                }
                else
                {
                    bool ok = DownloadSTLFiles(path, name);
                    if (!ok)
                    {
                        return JavaScript("alert(\"下载失败，请重新下载！\")");
                    }
                }
                model.ModelDownloadNumbers++;
                db.Entry(model).State = EntityState.Modified;
                db.SaveChanges();
                //add download record
                AddDownloadRecord(userId, modelId);
                return Redirect(Request.UrlReferrer.ToString());
            }
            else
            {
                return RedirectToAction("Login", "User");
            }
        }

        //if download more than two stl files, create .rar file and download
        public void CreateRarFile(string path, string name)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            Process process = new Process();

            //string rarName = "1.rar";
            //string filePath = @"G:\VisualStudio2015\OnlinePrint\OnlinePrint\Files\ModelLibrary\20160120160008195575";
            //string rarPath = @"G:\VisualStudio2015\OnlinePrint\OnlinePrint\Files\ModelLibrary\20160120160008195575";
            string rarExe = @"C:\Program Files\WinRAR\WinRAR.exe";
            string cmd = String.Format("a {0} {1} -r", name, path);
            startInfo.FileName = rarExe;
            startInfo.Arguments = cmd;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.WorkingDirectory = path;
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
            process.Dispose();
        }

        public bool DownloadSTLFiles(string path, string name)
        {
            const long chunk = 1024000;
            byte[] buffer = new byte[chunk];
            Response.ContentType = "application/octet-stream";
            using (System.IO.FileStream iStream = System.IO.File.OpenRead(path))
            {
                long dataLength = iStream.Length;
                Response.AddHeader("Content-Disposition", "attachment;filename=" + name);
                while (dataLength > 0 && Response.IsClientConnected)
                {
                    int lengthRead = iStream.Read(buffer, 0, Convert.ToInt32(chunk));
                    Response.OutputStream.Write(buffer, 0, lengthRead);
                    Response.Flush();
                    dataLength = dataLength - lengthRead;
                }
                Response.Close();
            }
            // Response.Clear();
            return true;
        }

        public void AddDownloadRecord(int userId, string modelId)
        {
            var record = db.DownloadRecord.Where(n => (n.UserID == userId) && (n.ModelID == modelId)).FirstOrDefault();
            if (record != null)
            {
                record.DownloadCounts++;
                db.Entry(record).State = EntityState.Modified;
                db.SaveChanges();
            }
            else
            {
                var newRecord = new DownloadRecord();
                newRecord.UserID = userId;
                newRecord.ModelID = modelId;
                newRecord.DownloadCounts = 1;
                newRecord.DownloadTime = DateTime.Now;
                db.DownloadRecord.Add(newRecord);
                db.SaveChanges();
            }
        }

        //if has downloaded return true, else false
        public bool HasDownloadFile(int userId, string modelId)
        {
            return db.DownloadRecord.Where(n => (n.UserID == userId) && (n.ModelID == modelId)) == null ? false : true;
        }

        //set filter paramerter in form
        private void SetFilterFormParameter(string filterTime, string orderBy, string minPrice, string maxPrice)
        {
            if (filterTime != null)
            {
                ViewBag.FilterTime = filterTime;
            }
            else
            {
                ViewBag.FilterTime = "";
            }

            if (orderBy != null)
            {
                ViewBag.OrderBy = orderBy;
            }
            else
            {
                ViewBag.OrderBy = "default";
            }
            if (minPrice != null)
            {
                ViewBag.MinPrice = minPrice;
            }
            else
            {
                ViewBag.MinPrice = "";
            }
            if (maxPrice != null)
            {
                ViewBag.MaxPrice = maxPrice;
            }
            else
            {
                ViewBag.MaxPrice = "";
            }
            return;
        }

        //filter category by parameter
        private PagingForFilter<ModelLibrary> FilterCategory(string modelClass, string filterTime, string minPrice, string maxPrice, string orderBy, int index)
        {
            if (modelClass == null)
            {
                return null;
            }

            if (filterTime == null && minPrice == null && maxPrice == null && orderBy == null)
            {
                SQLQuery = QueryString.queryNull;
            }
            else if (filterTime != null && minPrice == null && maxPrice == null && orderBy == null)
            {
                SQLQuery = QueryString.queryTime;
            }
            else if (filterTime == null && minPrice != null && maxPrice == null && orderBy == null)
            {
                SQLQuery = QueryString.queryMinprice;
            }
            else if (filterTime == null && minPrice == null && maxPrice != null && orderBy == null)
            {
                SQLQuery = QueryString.queryMaxprice;
            }
            else if (filterTime == null && minPrice == null && maxPrice == null && orderBy != null)
            {
                SQLQuery = QueryString.queryOrder;
            }
            else if (filterTime != null && minPrice != null && maxPrice == null && orderBy == null)
            {
                SQLQuery = QueryString.queryTimeMinprice;
            }
            else if (filterTime != null && minPrice == null && maxPrice != null && orderBy == null)
            {
                SQLQuery = QueryString.queryTimeMaxprice;
            }
            else if (filterTime != null && minPrice == null && maxPrice == null && orderBy != null)
            {
                SQLQuery = QueryString.queryTimeOrder;
            }
            else if (filterTime == null && minPrice != null && maxPrice == null && orderBy != null)
            {
                SQLQuery = QueryString.queryOrderMinprice;
            }
            else if (filterTime == null && minPrice == null && maxPrice != null && orderBy != null)
            {
                SQLQuery = QueryString.queryOrderMaxprice;
            }
            else if (filterTime == null && minPrice != null && maxPrice != null && orderBy == null)
            {
                SQLQuery = QueryString.queryMinpriceMaxprice;
            }
            else if (filterTime != null && minPrice != null && maxPrice != null && orderBy == null)
            {
                SQLQuery = QueryString.queryTimeMinpriceMaxprice;
            }
            else if (filterTime != null && minPrice != null && maxPrice == null && orderBy != null)
            {
                SQLQuery = QueryString.queryTimeOrderMinprice;
            }
            else if (filterTime != null && minPrice == null && maxPrice != null && orderBy != null)
            {
                SQLQuery = QueryString.queryTimeOrderMaxprice;
            }
            else if (filterTime == null && minPrice != null && maxPrice != null && orderBy != null)
            {
                SQLQuery = QueryString.queryOrderMinpriceMaxprice;
            }
            else if (filterTime != null && minPrice != null && maxPrice != null && orderBy != null)
            {
                SQLQuery = QueryString.queryTimeMinpriceMaxpriceOrder;
            }

            //exce linq to read data from sqlserver
            //var db = new OnlinePrintDbContext();
            PagingForFilter<ModelLibrary> modelPaging = new PagingForFilter<ModelLibrary>(8);
            IQueryable<ModelLibrary> models = null;

            if (modelClass == "all")
            {
                models = db.ModelLibrary.Where(n => n.ModelState);
            }
            else
            {
                models = db.ModelLibrary.Where(n => n.ModelState).Where(n => n.ModelClass == modelClass);
            }
            decimal min = 0;
            decimal max = 100000;

            switch (SQLQuery)
            {
                case QueryString.queryNull:
                    //modelPaging.PageData = models;
                    //modelPaging.PageIndex = index;
                    //modelPaging.CalCount();
                    modelPaging = SetModelPagingParam(index, models);
                    break;
                case QueryString.queryTime:
                    //modelPaging.PageData = FilterTime(filterTime, DateTime.Now, models);
                    //modelPaging.PageIndex = index;
                    //modelPaging.CalCount();
                    models = FilterTime(filterTime, DateTime.Now, models);
                    modelPaging = SetModelPagingParam(index, models);
                    break;
                case QueryString.queryMinprice:
                    min = Convert.ToDecimal(minPrice);
                    models = models.Where(n => (min <= n.ModelPrintPrice));
                    modelPaging = SetModelPagingParam(index, models);
                    break;
                case QueryString.queryMaxprice:
                    max = Convert.ToDecimal(maxPrice);
                    models = models.Where(n => (n.ModelPrintPrice <= max));
                    modelPaging = SetModelPagingParam(index, models);
                    break;
                case QueryString.queryOrder:
                    models = FilterOrderBy(orderBy, models);
                    modelPaging = SetModelPagingParam(index, models);
                    break;
                case QueryString.queryTimeMinprice:
                    min = Convert.ToDecimal(minPrice);
                    models = models.Where(n => (min <= n.ModelPrintPrice));
                    models = FilterTime(filterTime, DateTime.Now, models);
                    modelPaging = SetModelPagingParam(index, models);
                    break;
                case QueryString.queryTimeMaxprice:
                    max = Convert.ToDecimal(maxPrice);
                    models = models.Where(n => (n.ModelPrintPrice <= max));
                    models = FilterTime(filterTime, DateTime.Now, models);
                    modelPaging = SetModelPagingParam(index, models);
                    break;
                case QueryString.queryTimeOrder:
                    models = FilterTime(filterTime, DateTime.Now, models);
                    models = FilterOrderBy(orderBy, models);
                    modelPaging = SetModelPagingParam(index, models);
                    break;
                case QueryString.queryOrderMinprice:
                    min = Convert.ToDecimal(minPrice);
                    models = models.Where(n => (min <= n.ModelPrintPrice));
                    models = FilterOrderBy(orderBy, models);
                    modelPaging = SetModelPagingParam(index, models);
                    break;
                case QueryString.queryOrderMaxprice:
                    max = Convert.ToDecimal(maxPrice);
                    models = models.Where(n => (n.ModelPrintPrice <= max));
                    models = FilterOrderBy(orderBy, models);
                    modelPaging = SetModelPagingParam(index, models);
                    break;
                case QueryString.queryMinpriceMaxprice:
                    min = Convert.ToDecimal(minPrice);
                    max = Convert.ToDecimal(maxPrice);
                    models = models.Where(n => (min <= n.ModelPrintPrice) && (n.ModelPrintPrice <= max));
                    modelPaging = SetModelPagingParam(index, models);
                    break;
                case QueryString.queryTimeMinpriceMaxprice:
                    min = Convert.ToDecimal(minPrice);
                    max = Convert.ToDecimal(maxPrice);
                    models = FilterTime(filterTime, DateTime.Now, models);
                    models = models.Where(n => (min <= n.ModelPrintPrice) && (n.ModelPrintPrice <= max));
                    modelPaging = SetModelPagingParam(index, models);
                    break;
                case QueryString.queryTimeOrderMinprice:
                    min = Convert.ToDecimal(minPrice);
                    models = models.Where(n => (min <= n.ModelPrintPrice));
                    models = FilterOrderBy(orderBy, models);
                    modelPaging = SetModelPagingParam(index, models);
                    break;
                case QueryString.queryTimeOrderMaxprice:
                    max = Convert.ToDecimal(maxPrice);
                    models = models.Where(n => (n.ModelPrintPrice <= max));
                    models = FilterTime(filterTime, DateTime.Now, models);
                    models = FilterOrderBy(orderBy, models);
                    modelPaging = SetModelPagingParam(index, models);
                    break;
                case QueryString.queryOrderMinpriceMaxprice:
                    min = Convert.ToDecimal(minPrice);
                    max = Convert.ToDecimal(maxPrice);
                    models = models.Where(n => (min <= n.ModelPrintPrice) && (n.ModelPrintPrice <= max));
                    models = FilterOrderBy(orderBy, models);
                    modelPaging = SetModelPagingParam(index, models);
                    break;
                case QueryString.queryTimeMinpriceMaxpriceOrder:
                    min = Convert.ToDecimal(minPrice);
                    max = Convert.ToDecimal(maxPrice);
                    models = FilterTime(filterTime, DateTime.Now, models);
                    models = models.Where(n => (min <= n.ModelPrintPrice) && (n.ModelPrintPrice <= max));
                    models = FilterOrderBy(orderBy, models);
                    modelPaging = SetModelPagingParam(index, models);
                    break;
                default:
                    break;
            }
            return modelPaging;
        }
        //filter time
        private IQueryable<ModelLibrary> FilterTime(string filterTime, DateTime now, IQueryable<ModelLibrary> models)
        {
            IQueryable<ModelLibrary> model = null;
            switch (filterTime)
            {
                case "today":
                    model = models.Where(n => SqlFunctions.DateDiff("day", n.ModelCreatedTime, now) <= 1);
                    break;
                case "week":
                    model = models.Where(n => SqlFunctions.DateDiff("day", n.ModelCreatedTime, now) <= 7);
                    break;
                case "month":
                    model = models.Where(n => SqlFunctions.DateDiff("month", n.ModelCreatedTime, now) <= 1);
                    break;
                case "year":
                    model = models.Where(n => SqlFunctions.DateDiff("year", n.ModelCreatedTime, now) <= 1);
                    break;
                default:
                    break;
            }
            return model;
        }
        //filter orderby
        private IQueryable<ModelLibrary> FilterOrderBy(string orderBy, IQueryable<ModelLibrary> models)
        {
            IQueryable<ModelLibrary> model = null;
            switch (orderBy)
            {
                case "sales":
                    model = models.OrderBy(n => (n.ModelPrintNumbers));
                    break;
                case "price":
                    model = models.OrderBy(n => n.ModelPrintPrice);
                    break;
                case "newProduct":
                    model = models.OrderBy(n => (n.ModelCreatedTime));
                    break;
                default:
                    model = models;
                    break;
            }
            return model;
        }
        //set modelpaging parameter(index and source) and calculate counts
        private PagingForFilter<ModelLibrary> SetModelPagingParam(int index, IQueryable<ModelLibrary> models)
        {
            PagingForFilter<ModelLibrary> model = new PagingForFilter<ModelLibrary>(8);
            model.PageData = models;
            model.PageIndex = index;
            model.CalCount();
            return model;
        }
    }
}