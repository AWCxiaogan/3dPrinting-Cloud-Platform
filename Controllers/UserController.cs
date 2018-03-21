using System.Web;
using System.Web.Mvc;
using System.IO;
using OnlinePrint.Models;
using System;
using System.Linq;
using System.Web.Security;
using System.Data.Entity;
using System.Net;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.Remoting.Channels.Tcp;

using System.Threading;
using System.Text;

namespace OnlinePrint.Controllers
{
       enum OrderState : Int32
    {
        WaitPrice, //“已下单，待报价”
        WaitConfirm,//“已报价，待确认”
        WaitSubmitOrder,//“已确认，待提交”
        WaitPay,//“已提交，待支付”
        Succeed,//“交易成功”
        Canceled,//“已取消”
        Deleted,//“已删除”
    }
   
    public struct Volume
    {
        public int x;
        public int y;
        public int z;
        public int volume;
    }

    public struct Temp
    {
        public double tempX;
        public double tempY;
        public double tempZ;
        public Temp(double x, double y, double z)
        {
            tempX = x;
            tempY = y;
            tempZ = z;
        }
    }
    //验证码功能
    public class ValidateCode
    {
        public ValidateCode()
        {
        }
        public int MaxLength
        {
            get { return 10; }
        }
        public int MinLength
        {
            get { return 1; }
        }
        public string CreateValidateCode(int length)
        {
            int[] randMembers = new int[length];
            int[] validateNums = new int[length];
            string validateNumberStr = "";
            //unchecked can prevent overflow
            int seekSeek = unchecked((int)DateTime.Now.Ticks);
            Random seekRand = new Random(seekSeek);
            //int.MaxValue 
            int beginSeek = seekRand.Next(0, int.MaxValue - length * 10000);
            int[] seeks = new int[length];
            for (int i = 0; i < length; i++)
            {
                beginSeek += 10000;
                seeks[i] = beginSeek;
            }
            // use seeks[] to generate the random numbers
            for (int i = 0; i < length; i++)
            {
                Random rand = new Random(seeks[i]);
                int pownum = (int)Math.Pow(10, length);
                randMembers[i] = rand.Next(pownum, int.MaxValue);
            }
            //add the random to validateNums[]
            for (int i = 0; i < length; i++)
            {
                string numStr = randMembers[i].ToString();
                int numLength = numStr.Length;
                Random rand = new Random();
                int numPosition = rand.Next(0, numLength - 1);
                validateNums[i] = int.Parse(numStr.Substring(numPosition, 1));
            }
            // generate the validatenumbers
            for (int i = 0; i < length; i++)
            {
                validateNumberStr += validateNums[i].ToString();
            }
            return validateNumberStr;
        }

        // generate the image
        public byte[] CreateValidateGraphic(string validateCode)
        {
            Bitmap image = new Bitmap((int)Math.Ceiling(validateCode.Length * 12.0), 34);
            Graphics g = Graphics.FromImage(image);
            try
            {
                Random random = new Random();
                //clear the bg-color
                g.Clear(Color.White);
                //draw random lines
                for (int i = 0; i < 25; i++)
                {
                    int x1 = random.Next(image.Width);
                    int x2 = random.Next(image.Width);
                    int y1 = random.Next(image.Height);
                    int y2 = random.Next(image.Height);
                    g.DrawLine(new Pen(Color.Silver), x1, y1, x2, y2);
                }
                //define the fonts
                Font font = new Font("Arial", 12, (FontStyle.Bold | FontStyle.Italic));
                LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(0, 0, image.Width, image.Height),
                 Color.Blue, Color.DarkRed, 1.2f, true);
                g.DrawString(validateCode, font, brush, 3, 5);

                for (int i = 0; i < 100; i++)
                {
                    int x = random.Next(image.Width);
                    int y = random.Next(image.Height);
                    image.SetPixel(x, y, Color.FromArgb(random.Next()));
                }
                g.DrawRectangle(new Pen(Color.Silver), 0, 0, image.Width - 1, image.Height - 1);
                byte[] imageBytes = null;
                using (MemoryStream stream = new MemoryStream())
                {
                    image.Save(stream, ImageFormat.Jpeg);
                    imageBytes = stream.ToArray();
                }
                return imageBytes;
            }
            finally
            {
                g.Dispose();
                image.Dispose();
            }
        }
        public static int GetImageWidth(int validateNumLength)
        {
            return (int)(validateNumLength * 12.0);
        }
        public static double GetImageHeight()
        {
            return 22.5;
        }
    }

    public class UserController : Controller
    {
        // GET: User
        private OnlinePrintDbContext db;
        private OrderParameter orderParameter;

        public UserController()
        {
            db = new OnlinePrintDbContext();
            orderParameter = new OrderParameter();
        }

        //计算三维模型的实际体积
        public Volume CalculateVolumeOfSTL(string path)
        {
            //judge the format of the stl 
            string str1 = null;
            using (FileStream bFile = new FileStream(path, FileMode.Open))
            {
                BinaryReader abr = new BinaryReader(bFile);
                byte[] head1 = abr.ReadBytes(80);
                str1 = System.Text.Encoding.Default.GetString(head1);
                abr.Close();
            }
            //define the variable
            string Line;
            string finalLine;
            int faceNum = 0;
            double volume = 0.0;
            double xMax = 0, xMin = 0, xTem = 0, yMax = 0, yMin = 0, yTem = 0, zMax = 0, zMin = 0, zTem = 0;
            //asc-ii格式的三维模型计算步骤
            if (str1.Contains("facet"))
            {
                using (FileStream aFile = new FileStream(path, FileMode.Open))
                {
                    StreamReader str = new StreamReader(aFile);
                    while (!str.EndOfStream)
                    {
                        Line = str.ReadLine();
                        if (Line.Contains("endfacet"))
                        {
                            faceNum++;
                        }
                    }
                    str.Close();
                }
                using (FileStream cFile = new FileStream(path, FileMode.Open))
                {

                    StreamReader strb = new StreamReader(cFile);
                    finalLine = strb.ReadLine();
                    Temp o1 = new Temp(0, 0, 0);
                    Temp o2 = new Temp(0, 0, 0);
                    Temp o3 = new Temp(0, 0, 0);
                    int count = 0;
                    for (int i = 0; i < faceNum; i++)
                    {
                        count = 0;
                        while (!finalLine.Contains("endfacet"))
                        {
                            finalLine = strb.ReadLine();
                            if (finalLine.Contains("vertex"))
                            {
                                count++;
                                int num = finalLine.IndexOf('x') + 1;
                                finalLine = finalLine.Remove(0, num);
                                finalLine = finalLine.Trim(); //remove the blank characters 
                                string[] finalLineArr = finalLine.Split(' ');
                                if (count == 1)
                                {
                                    o1.tempX = Convert.ToDouble(finalLineArr[0]);
                                    o1.tempY = Convert.ToDouble(finalLineArr[1]);
                                    o1.tempZ = Convert.ToDouble(finalLineArr[2]);
                                    if (i == 0)
                                    {
                                        xMax = xMin = xTem = Convert.ToDouble(finalLineArr[0]);
                                        yMax = yMin = yTem = Convert.ToDouble(finalLineArr[1]);
                                        zMax = zMin = zTem = Convert.ToDouble(finalLineArr[2]);
                                    }
                                }
                                if (count == 2)
                                {
                                    o2.tempX = Convert.ToDouble(finalLineArr[0]);
                                    o2.tempY = Convert.ToDouble(finalLineArr[1]);
                                    o2.tempZ = Convert.ToDouble(finalLineArr[2]);
                                }
                                if (count == 3)
                                {
                                    o3.tempX = Convert.ToDouble(finalLineArr[0]);
                                    o3.tempY = Convert.ToDouble(finalLineArr[1]);
                                    o3.tempZ = Convert.ToDouble(finalLineArr[2]);
                                }
                                xTem = Convert.ToDouble(finalLineArr[0]);
                                yTem = Convert.ToDouble(finalLineArr[1]);
                                zTem = Convert.ToDouble(finalLineArr[2]);
                                if (xTem > xMax)
                                    xMax = xTem;
                                if (xTem < xMin)
                                    xMin = xTem;
                                if (yTem > yMax)
                                    yMax = yTem;
                                if (yTem < yMin)
                                    yMin = yTem;
                                if (zTem > zMax)
                                    zMax = zTem;
                                if (zTem < zMin)
                                    zMin = zTem;
                            }
                        }
                        finalLine = strb.ReadLine();
                        //空间不规则多面体 体积计算公式
                        volume += o1.tempX * o2.tempY * o3.tempZ + o1.tempY * o2.tempZ * o3.tempX + o1.tempZ * o2.tempX * o3.tempY - o1.tempX * o2.tempZ * o3.tempY - o1.tempZ * o2.tempY * o3.tempX - o1.tempY * o2.tempX * o3.tempZ;
                    }
                    strb.Close();
                }
            }
            //二进制格式的三维模型计算步骤
            else
            {
                using (FileStream aFile = new FileStream(path, FileMode.Open))
                {
                    BinaryReader br = new BinaryReader(aFile);
                    byte[] head = br.ReadBytes(80);
                    //read the next four bytes which means the number of the face
                    faceNum = Convert.ToInt32(br.ReadUInt32());
                    Temp o1 = new Temp(0, 0, 0);
                    Temp o2 = new Temp(0, 0, 0);
                    Temp o3 = new Temp(0, 0, 0);
                    for (int i = 0; i < faceNum; i++)
                    {
                        //foreach the angle vector
                        for (int j = 0; j < 3; j++)
                        {
                            br.ReadSingle();
                        }
                        //add the three coordinate to the List
                        for (int j = 0; j < 3; j++)
                        {
                            double v0 = br.ReadSingle();
                            double v1 = br.ReadSingle();
                            double v2 = br.ReadSingle();
                            if (j == 0)
                            {
                                o1 = new Temp(v0, v1, v2);
                                if (i == 0)
                                {
                                    xMax = xMin = xTem = v0;
                                    yMax = yMin = yTem = v1;
                                    zMax = zMin = zTem = v2;
                                }
                            }
                            if (j == 1)
                            {
                                o2 = new Temp(v0, v1, v2);
                            }
                            if (j == 2)
                            {
                                o3 = new Temp(v0, v1, v2);
                            }
                            xTem = v0;
                            yTem = v1;
                            zTem = v2;
                            if (xTem > xMax)
                                xMax = xTem;
                            if (xTem < xMin)
                                xMin = xTem;
                            if (yTem > yMax)
                                yMax = yTem;
                            if (yTem < yMin)
                                yMin = yTem;
                            if (zTem > zMax)
                                zMax = zTem;
                            if (zTem < zMin)
                                zMin = zTem;
                        }
                        //let the pointer skip the explaination
                        double test = br.ReadUInt16();
                        volume += o1.tempX * o2.tempY * o3.tempZ + o1.tempY * o2.tempZ * o3.tempX + o1.tempZ * o2.tempX * o3.tempY - o1.tempX * o2.tempZ * o3.tempY - o1.tempZ * o2.tempY * o3.tempX - o1.tempY * o2.tempX * o3.tempZ;
                    }
                    br.Close();
                }
            }
            //stlVolume代表三维模型包络体积   volume代表三维模型实际体积
            Volume stlVolume;
            volume = volume / 6;
            stlVolume.x = (int)(xMax - xMin);
            stlVolume.y = (int)(yMax - yMin);
            stlVolume.z = (int)(zMax - zMin);
            stlVolume.volume = (int)volume;
            return stlVolume;
        }
        //首页
        public ActionResult Index()
        {
            if (Session["UserID"] != null)
            {
                ViewBag.UserId = Session["UserID"];
                int userId = Convert.ToInt32(Session["UserID"].ToString());
                var user = db.Users.Where(n => n.UserID == userId).FirstOrDefault();
                ViewBag.UserName = user.UserName;
                ViewBag.ImagePath = user.UserImage;
                ViewBag.Information = "欢迎你" + user.UserName;
                return View();
            }
            else
            {
                return RedirectToAction("Login");
            }
        }
        public ActionResult Login()
        {
            return View();
        }
        //登录功能
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Login(string UserEmail, string UserPassword)
        {
            if (UserEmail == "" || UserPassword == "")
            {
                ModelState.AddModelError("", "请输入完整信息再登录！");
                return View();
            }
            else
            {
                if (ValidateUser(UserEmail, UserPassword))
                {
                    FormsAuthentication.SetAuthCookie(UserEmail, false);
                    return RedirectToAction("Index", "Home");
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
        public bool ValidateUser(string UserEmail, string UserPassword)
        {
            var user = db.Users.Where(n => (n.UserEmail == UserEmail) && (n.UserPassword == UserPassword)).FirstOrDefault();
            if (user != null)
            {
                Session["UserID"] = user.UserID;
                return true;
            }
            return false;
        }
        //注册功能
        public ActionResult Register()
        {
            return View();
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Register([Bind(Exclude = "UserID,UserGrade,UserQQ,UserDownloadTickets,UserRight,UserImage")]User user)
        {
            var check_user = db.Users.Where(p => p.UserEmail == user.UserEmail).FirstOrDefault();
            if (check_user != null)
            {
                ModelState.AddModelError("UserEmail", "您输入的邮箱已经有人注册过了");
            }
            if (ModelState.IsValid)
            {
                user.UserImage = "/Files/UserImage/defaultImage.png";//default image path
                db.Users.Add(user);

                //2018.1.11新增
                UserInfo userInfo = new UserInfo
                {
                    UserName = user.UserName,
                    UserPassword = user.UserPassword,
                    UserEmail = user.UserEmail,
                    UserCompany = user.UserCompany,
                    UserPhone = user.UserCellphone
                };
                db.UserInfo.Add(userInfo);
                db.SaveChanges();
                //if (db.WarehousePositionState.Find(0) == null)
                //{
                //    WarehousePositionState positionState = new WarehousePositionState
                //    {
                //        WarehousePositionId = 0,
                //        PositionState = 0
                //    };
                //    db.WarehousePositionState.Add(positionState);
                //}
                if (db.PrinterInfo.Find(0) == null)
                {
                    PrinterInfo printerInfo = new PrinterInfo
                    {
                        PrinterId = 0,
                        PrinterPrecision = 0.0f,
                        PrinterRangeX = 0,
                        PrinterRangeY = 0,
                        PrinterRangeZ = 0,
                        PrinterType = "",
                    };
                    db.PrinterInfo.Add(printerInfo);
                    db.Configuration.ValidateOnSaveEnabled = false;
                    db.SaveChanges();
                    db.Configuration.ValidateOnSaveEnabled = true;
                }

                
                return View("Login");
            }
            else
            {
                return View();
            }
        }
        //注销功能
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session["UserID"] = null;
            //Session.Clear();
            return View("Login");
        }
        //user's model
        public ActionResult Model(int index = 1)
        {
            //judge if user is logined
            if (Session["UserID"] != null)
            {
                ViewBag.UserName = Session["UserName"];
                ViewBag.UserId = Session["UserID"];
                // var db = new OnlinePrintDbContext();
                int userId = Convert.ToInt32(Session["UserID"].ToString());
                var user = db.Users.Where(n => n.UserID == userId).FirstOrDefault();
                ViewBag.UserName = user.UserName;
                ViewBag.ImagePath = user.UserImage;

                var userModels = from q in db.ModelLibrary
                                 join r in db.UsersModels on q.ModelID equals r.ModelID
                                 where r.UserID == userId
                                 select q;
                Paging<ModelLibrary> userModelsPage = new Paging<ModelLibrary>(6, userModels);
                userModelsPage.PageIndex = index;
                return View(userModelsPage);
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        //上传三维模型
        public ActionResult UploadModel()
        {
            if (Session["UserID"] != null)
            {
                ViewBag.UserName = Session["UserName"];
                ViewBag.UserId = Session["UserID"];
                // var db = new OnlinePrintDbContext();
                int userId = Convert.ToInt32(Session["UserID"].ToString());
                var user = db.Users.Where(n => n.UserID == userId).FirstOrDefault();
                ViewBag.UserName = user.UserName;
                ViewBag.ImagePath = user.UserImage;
                return View();
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        //上传用户头像
        [HttpPost]
        public ActionResult UploadImage(FormCollection form)
        {
            if (Session["UserID"] != null)
            {
                int userId = Convert.ToInt32(Session["UserID"]);
                var user = db.Users.Where(n => n.UserID == userId).FirstOrDefault();
                if (Request.Files.Count > 0)
                {
                    HttpPostedFileBase postFile = Request.Files[0];
                    string fileType = postFile.ContentType.ToLower();
                    string savePath = "";
                    //save image at file server
                    string defaultImagePath = "/Files/UserImage/" + Convert.ToString(user.UserID) + "/";
                    if (!Directory.Exists(Server.MapPath(defaultImagePath)))
                    {
                        Directory.CreateDirectory(Server.MapPath(defaultImagePath));
                    }
                    if (fileType.Contains("image"))
                    {
                        savePath = Path.Combine(Server.MapPath(defaultImagePath) + Path.GetFileName(postFile.FileName));
                    }
                    postFile.SaveAs(savePath);
                    //update database
                    var userPhotoName = Path.GetFileName(Request.Files["userImage"].FileName);
                    user.UserImage = defaultImagePath + userPhotoName;
                    db.Entry(user).State = EntityState.Modified;
                    db.SaveChanges();
                }
                return RedirectToAction("Information");
            }
            else
            {
                return RedirectToAction("Login");
            }
        }
        //验证码
        public ActionResult CheckCode()
        {
            ValidateCode validateCode = new ValidateCode();
            string code = validateCode.CreateValidateCode(4);
            System.Web.HttpContext.Current.Session["ValidateCode"] = code;
            byte[] bytes = validateCode.CreateValidateGraphic(code);
            return File(bytes, @"image/jpeg");
        }

        public ActionResult ValidateCheckCode(string input)
        {
            var validateCode = System.Web.HttpContext.Current.Session["ValidateCode"];
            if (validateCode.ToString() != input)
            {
                return Json(new { code = "" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { code = "ok" }, JsonRequestBehavior.AllowGet);
            }
        }
        //上传模型与模型图片功能
        //文件先通过JS调用的savestl函数 上传到temp文件夹中  该函数用于将temp文件夹中的文件转移到default文件夹中
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult UploadFile(FormCollection form)
        {
            if (Session["UserID"] != null)
            {
                int userId = Convert.ToInt32(Session["UserID"].ToString());
                //if no upload files or images, return 
                var uploadStlFileString = (form["uploadFiles"] != null ? form["uploadFiles"] : null);
                var uploadStlImageString = (form["uploadImages"] != null ? form["uploadImages"] : null);
                if ((uploadStlFileString == null) || (uploadStlImageString == null))
                {
                    return JavaScript("alert(\"请选择模型文件和图片\")");
                }
                //get upload files and images name
                List<string> stlFiles = new List<string>();
                foreach (var item in uploadStlFileString.Split(','))
                {
                    if (item != "")
                    {
                        stlFiles.Add(item);
                    }
                }
                List<string> images = new List<string>();
                foreach (var item in uploadStlImageString.Split(','))
                {
                    if (item != "")
                    {
                        images.Add(item);
                    }
                }
                //modelID
                Random ran = new Random();
                var randomID = ran.Next(100, 999);
                var modelID = DateTime.Now.ToString("yyyyMMddHHmmssfff") + randomID.ToString();
                //file path
                var defaultFilePath = "/Files/ModelLibrary/" + modelID + "/";
                var defaultImagePath = "/Files/Images/" + modelID + "/";
                var tempFilePath = "/Files/tempFiles/" + userId.ToString() + "/";
                var tempImagePath = "/Files/tempImages/" + userId.ToString() + "/";
                if (!Directory.Exists(Server.MapPath(defaultFilePath)))
                {
                    Directory.CreateDirectory(Server.MapPath(defaultFilePath));
                }
                if (!Directory.Exists(Server.MapPath(defaultImagePath)))
                {
                    Directory.CreateDirectory(Server.MapPath(defaultImagePath));
                }
                //move stl image files from temp directory to savePath
                for (int i = 0; i < stlFiles.Count; i++)
                {
                    //if stl file is not uploaded 100%, return and wait for upload
                   //如果不存在temp文件 则通知用户文件没有上传完毕
                    if (!System.IO.File.Exists(Server.MapPath(tempFilePath + stlFiles[i])))
                    {
                        Directory.Delete(Server.MapPath(defaultFilePath));
                        Directory.Delete(Server.MapPath(defaultImagePath));
                        return JavaScript("alert(\"文件没有上传完！\")");
                    }
                    //if defaultFilePath has file , do not move file, directly delete file in temp path
                    //如果存在default文件 则直接删除临时文件夹
                    if (System.IO.File.Exists(Server.MapPath(defaultFilePath + stlFiles[i])))
                    {
                        System.IO.File.Delete(Server.MapPath(tempFilePath + stlFiles[i]));
                    }
                    //如果不存在default文件 则将temp文件夹中的内容转移到default文件夹中
                    else
                    {
                        System.IO.File.Move(Server.MapPath(tempFilePath + stlFiles[i]),
                                        Server.MapPath(defaultFilePath + stlFiles[i]));
                    }
                }
                for (int i = 0; i < images.Count; i++)
                {
                    if (!System.IO.File.Exists(Server.MapPath(tempImagePath + images[i])))
                    {
                        break;
                    }
                    //if defaultImagePath has image , do not move image, directly delete image in temp path
                    if (System.IO.File.Exists(Server.MapPath(defaultImagePath + images[i])))
                    {
                        System.IO.File.Delete(Server.MapPath(tempImagePath + images[i]));
                    }
                    else
                    {
                        System.IO.File.Move(Server.MapPath(tempImagePath + images[i]),
                                        Server.MapPath(defaultImagePath + images[i]));
                    }
                }
                //add model info to modellibrary 
                var model = new ModelLibrary();
                model.ModelID = modelID;
                model.ModelName = form["modelName"];
                model.ModelClass = form["modelClass"];
                string keyWords = (form["keyWords"] != null ? form["keyWords"] : "");
                string[] words = keyWords.Split(',');
                model.ModelKeyWords = "";
                foreach (var word in words)
                {
                    model.ModelKeyWords += word;
                    model.ModelKeyWords += ",";
                }
                model.ModelDownloadNumbers = 0;
                model.ModelDownloadPrice = Convert.ToDecimal(form["downloadPrice"]);
                model.ModelPrintPrice = 0;
                model.ModelScanNumbers = 0;
                model.ModelPrintNumbers = 0;
                model.ModelScores = 5;
                var detail = form["description"];
                model.ModelDetail = ((form["description"] != null) ? form["description"] : "");
                model.ModelRight = form["modelRight"];
                model.ModelState = false;
                model.ModelCreatedTime = DateTime.Now;
                model.ModelPhotoPath = "***";
                model.ModelFileNames = "***";
                model.ModelPartCounts = 1;
                db.ModelLibrary.Add(model);
                db.SaveChanges();
                //add model details into modelPartDetail
                //if model has more than two parts
                if (stlFiles.Count < 2)
                {
                    var savedModel = db.ModelLibrary.Find(modelID);
                    savedModel.ModelFileNames = stlFiles.FirstOrDefault();
                    foreach (var item in images)
                    {
                        savedModel.ModelPhotoPath = defaultImagePath + item + ",";
                    }
                    db.Entry(savedModel).State = EntityState.Modified;
                    db.SaveChanges();
                    var modelDetail = new ModelPartsDetail();
                    modelDetail.ModelID = modelID;
                    modelDetail.ModelFileName = stlFiles.FirstOrDefault();
                    modelDetail.ModelPartPrintPrice = 0;
                    foreach (var item in images)
                    {
                        modelDetail.ModelPhotoPath = defaultImagePath + item + ",";
                    }
                    modelDetail.ModelPath = defaultFilePath + stlFiles.FirstOrDefault();
                    //get volume infomation
                    Volume volume = CalculateVolumeOfSTL(Server.MapPath(modelDetail.ModelPath));
                    modelDetail.Length = volume.x;
                    modelDetail.Width = volume.y;
                    modelDetail.Height = volume.z;
                    modelDetail.Volume = volume.volume;
                    db.ModelPartsDetail.Add(modelDetail);
                    db.SaveChanges();

                    //2018.1.11新增
                    ModelInfo modelInfo = new ModelInfo
                    {
                        UserId = userId,
                        ModelName = model.ModelName,
                        ModelKeyWords = model.ModelKeyWords,
                        ModelFileName = model.ModelFileNames,
                        ModelPhotoPath = model.ModelPhotoPath,
                        ModelDownloadTimes = model.ModelDownloadNumbers,
                        ModelPrintTimes = model.ModelPrintNumbers,
                        ModelFilePath = modelDetail.ModelPath,
                        ModelCreatedTime = DateTime.Now.ToLocalTime(),
                        OldModelId = model.ModelID
                    };
                    db.ModelInfo.Add(modelInfo);
                    db.SaveChanges();

                }
                else
                {
                    var savedModel = db.ModelLibrary.Find(modelID);
                    savedModel.ModelPartCounts = stlFiles.Count;
                    savedModel.ModelFileNames = "";
                    foreach (var item in stlFiles)
                    {
                        savedModel.ModelFileNames += item;
                        savedModel.ModelFileNames += ",";
                    }
                    savedModel.ModelPhotoPath = "";
                    for (int i = 0; i < images.Count; i++)
                    {
                        var isExistedStl = false;
                        var imageName = images[i].Substring(0, images[i].LastIndexOf('.'));
                        for (int j = 0; j < stlFiles.Count; j++)
                        {
                            var stlName = stlFiles[j].Substring(0, stlFiles[j].LastIndexOf('.'));
                            if (imageName == stlName)
                            {
                                isExistedStl = true;
                                break;
                            }
                        }
                        if (!isExistedStl)
                        {
                            savedModel.ModelPhotoPath += defaultImagePath + images[i] + ",";
                        }
                    }
                    db.Entry(savedModel).State = EntityState.Modified;
                    db.SaveChanges();

                    //add stl detail into modelPartsDetail
                    for (int i = 0; i < stlFiles.Count; i++)
                    {
                        var modelDetail = new ModelPartsDetail();
                        modelDetail.ModelID = modelID;
                        modelDetail.ModelFileName = stlFiles[i];
                        modelDetail.ModelPartPrintPrice = 0;
                        modelDetail.ModelPhotoPath = "***";
                        var stlName = stlFiles[i].Substring(0, stlFiles[i].LastIndexOf('.'));
                        for (int j = 0; j < images.Count; j++)
                        {
                            var imageName = images[j].Substring(0, images[j].LastIndexOf('.'));
                            if (stlName == imageName)
                            {
                                modelDetail.ModelPhotoPath = defaultImagePath + images[j];
                                break;
                            }
                        }
                        modelDetail.ModelPath = defaultFilePath + stlFiles[i];
                        //get volume infomation
                        Volume volume = CalculateVolumeOfSTL(Server.MapPath(modelDetail.ModelPath));
                        modelDetail.Length = volume.x;
                        modelDetail.Width = volume.y;
                        modelDetail.Height = volume.z;
                        db.ModelPartsDetail.Add(modelDetail);
                        db.SaveChanges();
                    }
                }
                //add model info to usermodel
                var userModel = new UserModel();
                userModel.UserID = Convert.ToInt16(Session["UserID"]);
                userModel.ModelID = modelID; ;
                userModel.ModelName = form["modelName"];
                db.UsersModels.Add(userModel);
                db.SaveChanges();

                return RedirectToAction("Model");
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        //get information of uploaded stl 断点续传时使用该函数
        public ActionResult GetFileInfo(string modelName)
        {
            if (Session["UserID"] != null)
            {
                int userId = Convert.ToInt32(Session["UserID"].ToString());
                var user = db.Users.Where(n => n.UserID == userId).FirstOrDefault();
                if (modelName == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.NotFound);
                }
                else
                {
                    var tempFilePath = "/Files/tempFiles/" + userId.ToString() + "/";
                    if (!Directory.Exists(Server.MapPath(tempFilePath)))
                    {
                        Directory.CreateDirectory(Server.MapPath(tempFilePath));
                    }
                    DirectoryInfo userTempDir = new DirectoryInfo(Server.MapPath(tempFilePath));
                    foreach (var file in userTempDir.GetFiles())
                    {
                        //judge ***.stl or ***.stl.temp is existed or not;
                        if ((file.Name == modelName) || ((file.Name.Substring(0, file.Name.Length - 5)) == modelName))
                        {
                            return Json(new { index = file.Length }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    return Json(new { index = 0 }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return RedirectToAction("Login");
            }
        }
        //断点续传JS中sendfile函数调用的服务器函数 用于将文件切割上传到temp文件中
        [HttpPost]
        public ActionResult SaveSTL()
        {
            if (Session["UserID"] != null)
            {
                int userId = Convert.ToInt32(Session["UserID"].ToString());
                var user = db.Users.Where(n => n.UserID == userId).FirstOrDefault();

                //temp stl and image file directory
                string tempFilePath = "/Files/tempFiles/" + userId.ToString() + "/";
                if (!Directory.Exists(Server.MapPath(tempFilePath)))
                {
                    Directory.CreateDirectory(Server.MapPath(tempFilePath));
                }
                //save uploaded file
                var data = Request.Files["data"];
                if (data == null)
                {
                    return null;
                }
                var size = Convert.ToInt32(Request["size"]);
                var dir = Server.MapPath(tempFilePath);
                var name = Request["name"];
                var fileName = name + ".temp";
                var file = Path.Combine(dir, fileName);
                //save stl file as ***.stl.temp first
                byte[] buffer = new byte[1024];
                using (FileStream fileStream = System.IO.File.Open(file, FileMode.Append))
                {
                    Stream inputFS = data.InputStream;
                    int dataLength = Convert.ToInt32(inputFS.Length);
                    while (dataLength > 0)
                    {
                        int dataRead = inputFS.Read(buffer, 0, 1024);
                        dataLength -= dataRead;
                        fileStream.Write(buffer, 0, dataRead);
                    }
                    inputFS.Close();
                }
                //fileStream.Close();
                //if finished, rename the file
                System.IO.FileInfo fileInfo = new System.IO.FileInfo(file);
                if (Convert.ToInt32(fileInfo.Length) == size)
                {
                    //var STLfile = file.Remove((file.Length - 5), 5);
                    var STLfile = file.Substring(0, file.Length - 4);
                    fileInfo.MoveTo(STLfile);
                };
                return Json(new { stat = "ok" });
            }
            else
            {
                return RedirectToAction("Login");
            }
        }
        //get information of uploaded stl image
        public ActionResult GetImageInfo(string imageName)
        {
            if (Session["UserID"] != null)
            {
                int userId = Convert.ToInt32(Session["UserID"].ToString());
                var user = db.Users.Where(n => n.UserID == userId).FirstOrDefault();
                if (imageName == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.NotFound);
                }
                else
                {
                    var tempImagePath = "/Files/tempImages/" + userId.ToString() + "/";
                    if (!Directory.Exists(Server.MapPath(tempImagePath)))
                    {
                        Directory.CreateDirectory(Server.MapPath(tempImagePath));
                    }
                    DirectoryInfo userTempDir = new DirectoryInfo(Server.MapPath(tempImagePath));
                    foreach (var file in userTempDir.GetFiles())
                    {
                        //judge ***.jpeg is existed or not;
                        if ((file.Name == imageName)
                            || (file.Name.Substring(0, file.Name.IndexOf('.')) == imageName.Substring(0, imageName.IndexOf('.'))))
                        {
                            return Json(new { state = "existed", path = tempImagePath + imageName }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    return Json(new { state = "notExisted", path = "" }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return RedirectToAction("Login");
            }
        }
        [HttpPost]
        public ActionResult SaveImage()
        {
            if (Session["UserID"] != null)
            {
                int userId = Convert.ToInt32(Session["UserID"].ToString());
                var user = db.Users.Where(n => n.UserID == userId).FirstOrDefault();
                string tempImagePath = "/Files/tempImages/" + userId.ToString() + "/";
                if (!Directory.Exists(Server.MapPath(tempImagePath)))
                {
                    Directory.CreateDirectory(Server.MapPath(tempImagePath));
                }
                var image = Request.Files["data"];
                if (image == null)
                {
                    return null;
                }

                var imageName = image.FileName;
                var savePath = Path.Combine(Server.MapPath(tempImagePath), imageName);
                image.SaveAs(savePath);
                return Json(new { path = tempImagePath + imageName });
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        //删除模型功能
        public ActionResult DeleteModel(string modelId)
        {
            if (Session["UserID"] != null)
            {
                var model = db.UsersModels.Where(n => n.ModelID == modelId).FirstOrDefault();
                db.UsersModels.Remove(model);
                db.SaveChanges();
                return RedirectToAction("Model");
            }
            else
            {
                return RedirectToAction("Login");
            }
        }
        //修改模型功能
        public ActionResult ModifyModel(string modelId)
        {
            //judge if user is logined
            if (Session["UserID"] != null)
            {
                ViewBag.UserId = Session["UserID"];
                int userId = Convert.ToInt32(Session["UserID"].ToString());
                var user = db.Users.Where(n => n.UserID == userId).FirstOrDefault();
                var model = db.ModelLibrary.Where(n => n.ModelID == modelId).FirstOrDefault();
                ViewData.Model = model;
                ViewBag.UserName = user.UserName;
                ViewBag.imagePath = user.UserImage;
                return View();
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        [HttpPost]
        public ActionResult ModifyModel(FormCollection form)
        {
            if (Session["UserID"] != null)
            {
                var modelId = form["modelId"];
                string defaultImagePath = "/Files/Images/" + modelId + "/";
                if (!Directory.Exists(Server.MapPath(defaultImagePath)))
                {
                    Directory.CreateDirectory(Server.MapPath(defaultImagePath));
                }
                for (int i = 0; i < Request.Files.Count; i++)
                {
                    HttpPostedFileBase postFile = Request.Files[i];//get file by index
                    string fileType = postFile.ContentType.ToLower();
                    string savePath = "";
                    savePath = Path.Combine(defaultImagePath, Path.GetFileName(postFile.FileName));
                    if (postFile.ContentLength == 0)
                        continue;
                    postFile.SaveAs(savePath);
                }
                var model = db.ModelLibrary.FirstOrDefault(n => n.ModelID == modelId);
                model.ModelClass = form["modifyModelClass"];
                model.ModelName = form["modifyModelName"];
                model.ModelDownloadPrice = Convert.ToDecimal(form["modifyDownloadPrice"]);
                model.ModelKeyWords = form["modifyKeyWords"];
                model.ModelDetail = form["modifyDescription"];
                model.ModelRight = form["modifyModelRight"];
                if (Request.Files.Count > 0)
                {
                    var modelPhotoName = Path.GetFileName(Request.Files["fileImage"].FileName);
                    //model.ModelPhotoPath = Server.MapPath(defaultImagePath) + modelPhotoName;
                    model.ModelPhotoPath = defaultImagePath + modelPhotoName;
                }
                db.SaveChanges();//update model infomation
                return RedirectToAction("Model");
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

       

        //user's order
        public ActionResult Order(int index = 1)
        {
            //judge if user is logined
            if (Session["UserID"] != null)
            {
                ViewBag.UserId = Session["UserID"];
                int userId = Convert.ToInt32(Session["UserID"].ToString());
                var user = db.Users.Where(n => n.UserID == userId).FirstOrDefault();
                ViewBag.ImagePath = user.UserImage;
                ViewBag.UserName = user.UserName;
                //get orderItems
                var orderItems = new List<OrderItem>();
                //select not deleted orders
                var orders = db.Orders.Where(n => n.UserID == userId).Where(n => n.State != (Int32)OrderState.Deleted).OrderByDescending(n => n.OrderID);
                foreach (var order in orders)
                {
                    var orderItem = new OrderItem();
                    orderItem.Order = order;
                    var items = db.OrderDetail.Where(n => n.OrderID == order.OrderID);
                    //add orderDetail to orderItems
                    foreach (var item in items)
                    {
                        orderItem.Items.Add(item);
                    }
                    orderItems.Add(orderItem);
                }
                Paging<OrderItem> userOrders = new Paging<OrderItem>(2, orderItems);
                userOrders.PageIndex = index;
                return View(userOrders);
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        //delete order
        public ActionResult DeleteOrder(string orderId)
        {
            if (Session["UserID"] != null)
            {
                ViewBag.UserId = Session["UserID"];
                int userId = Convert.ToInt32(Session["UserID"].ToString());
                var user = db.Users.Where(n => n.UserID == userId).FirstOrDefault();
                ViewBag.ImagePath = user.UserImage;
                ViewBag.UserName = user.UserName;

                var order = db.Orders.Where(n => n.OrderID == orderId).FirstOrDefault();
                switch (order.State)
                {
                    case (Int32)OrderState.WaitPrice:
                        db.Orders.Remove(order);
                        db.Entry(order).State = EntityState.Deleted;
                        break;
                    case (Int32)OrderState.WaitConfirm:
                        db.Orders.Remove(order);
                        db.Entry(order).State = EntityState.Deleted;
                        break;
                    case (Int32)OrderState.WaitSubmitOrder:
                        db.Orders.Remove(order);
                        db.Entry(order).State = EntityState.Deleted;
                        break;
                    case (Int32)OrderState.WaitPay:
                        order.State = (Int32)OrderState.Deleted;
                        db.Entry(order).State = EntityState.Modified;
                        break;
                    case (Int32)OrderState.Succeed:
                        order.State = (Int32)OrderState.Deleted;
                        db.Entry(order).State = EntityState.Modified;
                        break;
                    case (Int32)OrderState.Canceled:
                        db.Orders.Remove(order);
                        db.Entry(order).State = EntityState.Deleted;
                        break;
                    default:
                        break;
                }
                db.SaveChanges();
                return RedirectToAction("Order");
            }
            else
            {
                return RedirectToAction("Login");

            }
        }

        //cancel order
        public ActionResult CancelOrder(string orderId)
        {
            if (Session["UserID"] != null)
            {
                ViewBag.UserId = Session["UserID"];
                int userId = Convert.ToInt32(Session["UserID"].ToString());
                var user = db.Users.Where(n => n.UserID == userId).FirstOrDefault();
                ViewBag.ImagePath = user.UserImage;
                ViewBag.UserName = user.UserName;

                var order = db.Orders.Where(n => n.OrderID == orderId).FirstOrDefault();
                order.State = (Int32)OrderState.Canceled;
                db.SaveChanges();
                return RedirectToAction("Order");
            }
            else
            {
                return RedirectToAction("Login");

            }
        }

        //confirm order and submit 
        public ActionResult ConfirmOrder(string orderId)
        {
            if (Session["UserID"] != null)
            {
                ViewBag.UserId = Session["UserID"];
                int userId = Convert.ToInt32(Session["UserID"].ToString());
                var user = db.Users.Where(n => n.UserID == userId).FirstOrDefault();
                ViewBag.ImagePath = user.UserImage;
                ViewBag.UserName = user.UserName;

                Submit submit = new Submit();
                var order = db.Orders.Where(n => n.OrderID == orderId).FirstOrDefault();
                submit.order = order;
                order.State = (Int32)OrderState.WaitSubmitOrder;
                db.Entry(order).State = EntityState.Modified;
                db.SaveChanges();

                var orderDetails = db.OrderDetail.Where(n => n.OrderID == orderId);
                foreach (var orderdetail in orderDetails)
                {
                    submit.orderdetail.Add(orderdetail);
                }
                var address = db.DeliveryAddress.Where(n => n.UserID == userId).ToArray();
                for (int i = 0; i < address.Count(); i++)
                {
                    submit.address.Add(address[i]);
                }
                return View("Submit", submit);
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        //system message
        public ActionResult Message()
        {
            //judge if user is logined
            if (Session["UserID"] != null)
            {
                ViewBag.UserId = Session["UserID"];
                int userId = Convert.ToInt32(Session["UserID"].ToString());
                var user = db.Users.Where(n => n.UserID == userId).FirstOrDefault();
                ViewBag.ImagePath = user.UserImage;
                ViewBag.UserName = user.UserName;

                var userMessage = db.UserMessage.Where(n => n.UserID == userId).Where(n => n.State == false);
                return View(userMessage);
            }
            else
            {
                return RedirectToAction("Login");
            }
        }
        //delete message
        public ActionResult DeleteMessage(string messageID)
        {
            if (Session["UserID"] != null)
            {
                UserMessage userMessage = db.UserMessage.Where(n => n.MessageID == messageID).FirstOrDefault();
                db.UserMessage.Remove(userMessage);
                db.Entry(userMessage).State = EntityState.Deleted;
                db.SaveChanges();
                return RedirectToAction("Message");
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        //user's customize
        public ActionResult Customize()
        {
            //judge if user is logined
            if (Session["UserID"] != null)
            {
                ViewBag.UserId = Session["UserID"];
                int userId = Convert.ToInt32(Session["UserID"].ToString());
                var user = db.Users.Where(n => n.UserID == userId).FirstOrDefault();
                ViewBag.ImagePath = user.UserImage;
                ViewBag.UserName = user.UserName;
                return View();
            }
            else
            {
                return RedirectToAction("Login");
            }
        }
        //购物车功能
        [HttpPost]
        public ActionResult AddCart(FormCollection form)
        {
            if (Session["UserID"] != null)
            {
                //show user info
                ViewBag.UserId = Session["UserID"];
                int userId = Convert.ToInt32(Session["UserID"].ToString());
                var user = db.Users.Where(n => n.UserID == userId).FirstOrDefault();
                ViewBag.UserName = user.UserName;
                string requestUrl = Request.UrlReferrer.ToString();
                //modify cart, print parameter
                string modelId = form["modelID"];
                string material = form["printMaterial"];
                string color = form["printColor"];
                string precision = form["printPrecision"];
                string optionalRatio = form["optionalRatio"] != null ? form["optionalRatio"] : "1";
                string fillRate = form["fillRate"];
                int count = Convert.ToInt32(form["buyCount"]);
                var modelDetails = db.ModelPartsDetail.Where(n => n.ModelID == modelId);
                var model = db.ModelLibrary.Where(n => n.ModelID == modelId).FirstOrDefault();
                foreach (var item in modelDetails)
                {
                    var usercart = new UserCart();
                    usercart.PrintParameter = "材料:" + material + ";" + "颜色:" + color + ";" + "打印精度:" + precision + ";" + "打印强度:" + fillRate + ";" + "缩放比例:" + optionalRatio + ";";
                    usercart.ModelID = modelId;
                    usercart.UserID = userId;
                    //usercart.ModelClass = item.ModelClass;
                    usercart.ModelName = model.ModelName;
                    usercart.ModelFileName = item.ModelFileName;
                    //usercart.ModelDetail = models.ModelDetail;
                    usercart.ModelPrintPrice = item.ModelPartPrintPrice;
                    var order = new Orders();
                    //UserCart中是否已经有该用户相应模型ID和打印参数的购物记录
                    var check_cart = db.UserCart.Where(n => (n.ModelID == usercart.ModelID) && (n.UserID == userId) &&
                                                            (n.PrintParameter == usercart.PrintParameter)).FirstOrDefault();
                    if (check_cart != null)
                    {
                        //update usercart
                        check_cart.ModelNumber = count;
                        check_cart.TotalPrice = check_cart.ModelPrintPrice * check_cart.ModelNumber;
                        db.Entry(check_cart).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    else
                    {
                        //add new item to usercart
                        usercart.ModelNumber = count;
                        usercart.TotalPrice = usercart.ModelPrintPrice * usercart.ModelNumber;
                        db.UserCart.Add(usercart);
                    }
                }
                db.SaveChanges();
                return Redirect(requestUrl);
            }
            else
            {
                return RedirectToAction("Login", "User");
            }
        }

        [HttpPost]
        public ActionResult AddPartCart(FormCollection form)
        {
            if (Session["UserID"] != null)
            {
                //show user info
                ViewBag.UserId = Session["UserID"];
                int userId = Convert.ToInt32(Session["UserID"].ToString());
                var user = db.Users.Where(n => n.UserID == userId).FirstOrDefault();
                ViewBag.UserName = user.UserName;
                string requestUrl = Request.UrlReferrer.ToString();
                //modify cart, print parameter
                string modelId = form["modelID"];
                string material = form["printMaterial"];
                string color = form["printColor"];
                string precision = form["printPrecision"];
                string optionalRatio = form["optionalRatio"];
                string fillRate = form["fillRate"];
                int count = Convert.ToInt32(form["buyCount"]);
                string partName = form["partName"];
                var usercart = new UserCart();
                usercart.PrintParameter = "材料:" + material + ";" + "颜色:" + color + ";" + "打印精度:" + precision + ";" + "打印强度:" + fillRate + ";" + "缩放比例:" + optionalRatio + ";";
                var models = db.ModelLibrary.Where(n => n.ModelID == modelId).FirstOrDefault();
                usercart.ModelID = modelId;
                usercart.UserID = userId;
                //usercart.ModelClass = models.ModelClass;
                usercart.ModelName = models.ModelName;
                //usercart.ModelDetail = models.ModelDetail;
                usercart.ModelPrintPrice = models.ModelPrintPrice;
                usercart.ModelFileName = partName;
                var order = new Orders();
                //UserCart中是否已经有该用户相应模型ID和打印参数的购物记录
                var check_cart = db.UserCart.Where(n => (n.ModelID == usercart.ModelID) && (n.UserID == userId) &&
                                                        (n.PrintParameter == usercart.PrintParameter)).FirstOrDefault();
                if (check_cart != null)
                {
                    //update usercart
                    check_cart.ModelNumber = count;
                    check_cart.TotalPrice = check_cart.ModelPrintPrice * check_cart.ModelNumber;
                    db.Entry(check_cart).State = EntityState.Modified;
                    db.SaveChanges();
                }
                else
                {
                    //add new item to usercart
                    usercart.ModelNumber = count;
                    usercart.TotalPrice = usercart.ModelPrintPrice * usercart.ModelNumber;
                    db.UserCart.Add(usercart);
                    db.SaveChanges();
                }
                //return RedirectToAction("CacheView");
                return Redirect(requestUrl);
            }
            else
            {
                return RedirectToAction("Login", "User");
            }
        }

        public ActionResult DeleteCart(string ModelID, string PrintParameter)
        {
            if (Session["UserID"] != null)
            {
                ViewBag.UserId = Session["UserID"];
                int userId = Convert.ToInt32(Session["UserID"].ToString());
                var user = db.Users.Where(n => n.UserID == userId).FirstOrDefault();
                ViewBag.ImagePath = user.UserImage;
                ViewBag.UserName = user.UserName;

                UserCart usercart = db.UserCart.Where(n => (n.ModelID == ModelID) && (n.PrintParameter == PrintParameter)).FirstOrDefault();
                db.UserCart.Remove(usercart);
                db.Entry(usercart).State = EntityState.Deleted;
                db.SaveChanges();
                return RedirectToAction("Cart", "User");
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        //user's cart
        public ActionResult Cart()
        {
            //judge if user is logined
            if (Session["UserID"] != null)
            {
                ViewBag.UserId = Session["UserID"];
                int userId = Convert.ToInt32(Session["UserID"].ToString());
                var user = db.Users.Where(n => n.UserID == userId).FirstOrDefault();
                ViewBag.ImagePath = user.UserImage;
                ViewBag.UserName = user.UserName;

                var usercart = db.UserCart.Where(n => n.UserID == userId);
                return View(usercart);
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        //submit cart
        [HttpPost]
        public ActionResult SubmitCart(FormCollection form)
        {
            if (Session["UserID"] != null)
            {
                int userId = Convert.ToInt32(Session["UserID"].ToString());
                var user = db.Users.Where(n => n.UserID == userId).FirstOrDefault();
                ViewBag.UserID = Session["UserID"];
                ViewBag.UserName = user.UserName;

                string modelId = form["modelId"];
                string modelParam = form["printParameter"];
                string[] ids = modelId.Split(',');
                string[] parameters = modelParam.Split(',');
                //file names
                var fileName = form["fileName"];
                var names = fileName.Split(',');
                decimal sum = 0;
                Submit submit = new Submit();
                //order
                Orders order = new Orders();
                order.UserID = userId;
                Random random = new Random();
                order.OrderID = DateTime.Now.ToString("yyyyMMddHHmmss") + Convert.ToString(random.Next(1000, 9999));
                order.OrderTime = System.DateTime.Now;
                order.State = (Int32)OrderState.WaitPrice;
                var userAddress = db.DeliveryAddress.Where(n => (n.UserID == userId) && (n.DefaultAddress == true)).FirstOrDefault();
                if (userAddress != null)
                {
                    order.DeliveryAddress = userAddress.DeliveryAddress;
                    order.ReceiverCellphone = userAddress.ReceiverCellphone;
                }
                else
                {
                    order.DeliveryAddress = "***";
                    order.ReceiverCellphone = "***";
                }
                order.Sums = 0;
                db.Orders.Add(order);
                db.SaveChanges();

                bool waitPrice = false;

                //add OrderDetails
                for (int i = 0; i < ids.Count(); i++)
                {
                    string id = ids[i];
                    string param = parameters[i];
                    string name = names[i];
                    //if model's print parameter is adjusted, caluculat new print price
                    string[] paramDetails = param.Split(new char[] { ';', ':' });
                    string precision = paramDetails[4];
                    string fillRate = paramDetails[7];
                    string optionalRadio = paramDetails[9];
                    var usercart = db.UserCart.Where(n => (n.ModelID == id) && (n.PrintParameter == param) &&
                                                          (n.UserID == userId) && (n.ModelFileName == name)).FirstOrDefault();
                    //save order detail
                    OrderDetail orderdetail = new OrderDetail();
                    orderdetail.OrderID = order.OrderID;
                    orderdetail.Price = usercart.ModelPrintPrice;
                    orderdetail.PrintParameter = usercart.PrintParameter;
                    orderdetail.ModelName = usercart.ModelName;
                    orderdetail.ModelFileName = usercart.ModelFileName;
                    orderdetail.ModelID = usercart.ModelID;
                    orderdetail.Counts = usercart.ModelNumber;
                    orderdetail.Sums = usercart.TotalPrice;
                    orderdetail.State = (Int32)OrderState.WaitPay;
                    bool differentParam = ((precision != "低") || (fillRate != "一般") || (optionalRadio != "1")) ? true : false;
                    if (differentParam)
                    {
                        //if order detail record is  not exist, calculate new print price
                        var orderDetail = db.OrderDetail.Where(n => (n.ModelID == id) && (n.PrintParameter == param)
                                                              && (n.ModelFileName == name)).OrderByDescending(n => n.OrderID).FirstOrDefault();
                        if (orderDetail != null)
                        {
                            orderdetail.Price = orderDetail.Price;
                            orderdetail.Sums = orderdetail.Counts * orderdetail.Price;
                            orderdetail.State = (Int32)OrderState.WaitPay;
                            sum += orderdetail.Sums;
                        }
                        else
                        {
                            waitPrice = true;
                            orderdetail.State = (Int32)OrderState.WaitPrice;
                        }
                    }
                    else
                    {
                        sum += orderdetail.Sums;
                    }
                    db.OrderDetail.Add(orderdetail);
                    db.SaveChanges();
                    //show orderdetail in view
                    submit.orderdetail.Add(orderdetail);
                    //delete usercart
                    db.Entry(usercart).State = EntityState.Deleted;
                    db.SaveChanges();
                }
                if (waitPrice)
                {
                    order.State = (Int32)OrderState.WaitPrice;
                    sum = 0;
                }
                else
                {
                    order.State = (Int32)OrderState.WaitSubmitOrder;
                }
                order.Sums = sum;
                db.Entry(order).State = EntityState.Modified;
                db.SaveChanges();
                submit.order = order;

                var address = db.DeliveryAddress.Where(n => n.UserID == userId).ToArray();
                for (int i = 0; i < address.Count(); i++)
                {
                    submit.address.Add(address[i]);
                }
                switch (order.State)
                {
                    case (Int32)OrderState.WaitPrice:
                        return RedirectToAction("Order");
                    case (Int32)OrderState.WaitSubmitOrder:
                        return View("Submit", submit);
                    default:
                        return RedirectToAction("Order");
                }
            }
            else
            {
                return RedirectToAction("Login");
            }
        }       
//缓冲页面
        public ActionResult CacheView()
        {
            if (Session["UserID"] != null)
            {
                int userId = Convert.ToInt32(Session["UserID"].ToString());
                var user = db.Users.Where(n => n.UserID == userId).FirstOrDefault();
                ViewBag.UserID = Session["UserID"];
                ViewBag.UserName = user.UserName;
                ViewBag.ImagePath = user.UserImage;
                return View();
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        public ActionResult EditCart(string ModelID, string PrintParameter)
        {
            if (Session["UserID"] != null)
            {
                int userId = Convert.ToInt32(Session["UserID"].ToString());
                var user = db.Users.Where(n => n.UserID == userId).FirstOrDefault();
                ViewBag.UserID = Session["UserID"];
                ViewBag.UserName = user.UserName;
                ViewBag.ImagePath = user.UserImage;
                UserCart usercart = db.UserCart.Where(n => (n.ModelID == ModelID) && (n.PrintParameter == PrintParameter)).FirstOrDefault();
                return View(usercart);
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        [HttpPost]
        public ActionResult EditCart(FormCollection form)
        {

            if (Session["UserID"] != null)
            {
                int userId = Convert.ToInt32(Session["UserID"].ToString());
                var user = db.Users.Where(n => n.UserID == userId).FirstOrDefault();
                ViewBag.UserID = Session["UserID"];
                ViewBag.UserName = user.UserName;
                ViewBag.ImagePath = user.UserImage;
                string modelId = form["modelId"];
                string parameter = form["parameter"];
                string count = form["count"];
                var usercart = db.UserCart.Where(n => (n.ModelID == modelId) && (n.PrintParameter == parameter) &&
                                                      (n.UserID == userId)).FirstOrDefault();

                usercart.ModelNumber = Convert.ToInt32(count);
                usercart.TotalPrice = usercart.ModelPrintPrice * usercart.ModelNumber;
                db.Entry(usercart).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Cart");
            }
            else
            {
                return RedirectToAction("Login");
            }
        }
        //用户个人信息
        public ActionResult Information()
        {
            //judge if user is logined
            if (Session["UserID"] != null)
            {
                ViewBag.UserId = Session["UserID"];
                int userId = Convert.ToInt32(Session["UserID"].ToString());
                var user = db.Users.Where(n => n.UserID == userId).FirstOrDefault();
                ViewBag.UserName = user.UserName;
                ViewBag.ImagePath = user.UserImage;
                return View(user);
            }
            else
            {
                return RedirectToAction("Login");
            }
        }
        //edit user's information
        public ActionResult EditInformation(int? UserID)
        {
            if (UserID == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
                User user = db.Users.Find(UserID);
                if (user == null)
                {
                    return HttpNotFound();
                }
                else
                {
                    if (Session["UserID"] != null)
                    {
                        ViewBag.UserId = Session["UserID"];
                        int userId = Convert.ToInt32(Session["UserID"].ToString());
                        ViewBag.ImagePath = user.UserImage;
                        ViewBag.UserName = user.UserName;
                        return View(user);
                    }
                    else
                    {
                        return RedirectToAction("Login");
                    }
                }
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditInformation(User user)
        {
            if (ModelState.IsValid)
            {
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(user);
        }
        //user's address
        public ActionResult Address()
        {
            //judge if user is logined
            if (Session["UserID"] != null)
            {
                ViewBag.UserId = Session["UserID"];
                int userId = Convert.ToInt32(Session["UserID"].ToString());
                var user = db.Users.Where(n => n.UserID == userId).FirstOrDefault();
                ViewBag.ImagePath = user.UserImage;
                ViewBag.UserName = user.UserName;
                var address = db.DeliveryAddress.Where(n => n.UserID == userId);
                return View(address);
            }
            else
            {
                return RedirectToAction("Login");
            }
        }
            
        public ActionResult EditAddress(int? UserID, string ReceiverCellphone, string DeliveryAddress)
        {
            if (UserID == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (Session["UserID"] != null)
            {
                ViewBag.UserId = Session["UserID"];
                int userId = Convert.ToInt32(Session["UserID"].ToString());
                var user = db.Users.Find(userId);
                ViewBag.ImagePath = user.UserImage;
                ViewBag.UserName = user.UserName;
                UserDeliveryAddress useraddress = db.DeliveryAddress.Find(ReceiverCellphone, DeliveryAddress);
                if (useraddress == null)
                {
                    return HttpNotFound();
                }
                return View(useraddress);
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        [HttpPost]
        public ActionResult EditAddress(UserDeliveryAddress useraddress, string oldCellphone, string oldAddress)
        {
            if (ModelState.IsValid)
            {
                string ReceiverCellphone = oldCellphone;
                string DeliveryAddress = oldAddress;
                UserDeliveryAddress useraddress1 = db.DeliveryAddress.Find(ReceiverCellphone, DeliveryAddress);
                db.Entry(useraddress1).State = EntityState.Deleted;
                db.Entry(useraddress).State = EntityState.Added;
                db.SaveChanges();
                return RedirectToAction("Address");
            }
            return View(useraddress);
        }

        public ActionResult Delete(int UserID, string ReceiverCellphone, string DeliveryAddress)
        {
            if (Session["UserID"] != null)
            {
                UserDeliveryAddress useraddress = db.DeliveryAddress.Find(ReceiverCellphone, DeliveryAddress);
                db.DeliveryAddress.Remove(useraddress);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        public ActionResult CreateNewAddress()
        {
            if (Session["UserID"] != null)
            {
                ViewBag.UserId = Session["UserID"];
                int userId = Convert.ToInt32(Session["UserID"].ToString());
                var user = db.Users.Find(userId);
                ViewBag.ImagePath = user.UserImage;
                ViewBag.UserName = user.UserName;
                return View();
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateNewAddress(UserDeliveryAddress useraddress)
        {
            if (Session["UserID"] != null)
            {
                if (ModelState.IsValid)
                {

                    if (useraddress.DefaultAddress == true)
                    {
                        var deliveryaddresses = db.DeliveryAddress.Where(n => (n.UserID == useraddress.UserID) && (n.DefaultAddress == true)).FirstOrDefault();
                        if (deliveryaddresses != null)
                        {
                            deliveryaddresses.DefaultAddress = false;
                            db.Entry(deliveryaddresses).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }
                    useraddress.UserID = Convert.ToInt32(Session["UserID"]);
                    db.DeliveryAddress.Add(useraddress);
                    db.SaveChanges();
                    return RedirectToAction("Address");
                }
                return View(useraddress);

            }
            else
            {
                return RedirectToAction("Login");
            }

        }

        [HttpPost]
        public ActionResult AddOrder(FormCollection form)
        {
            if (Session["UserID"] != null)
            {
                ViewBag.UserId = Session["UserID"];
                int userId = Convert.ToInt32(Session["UserID"].ToString());
                var user = db.Users.Find(userId);
                ViewBag.UserName = user.UserName;

                string modelId = form["modelID"];
                string material = form["printMaterial"];
                string color = form["printColor"];
                string precision = form["printPrecision"];
                string fillRate = form["fillRate"];
                string optionalRatio = form["optionalRatio"] != null ? form["optionalRatio"] : "1";
                int count = Convert.ToInt32(form["buyCount"]);

                var order = new Orders();
                order.UserID = userId;
                Random random = new Random();
                var orderId = DateTime.Now.ToString("yyyyMMddHHmmss") + Convert.ToString(random.Next(1000, 9999));
                order.OrderID = orderId;
                order.OrderTime = System.DateTime.Now;
                var userAddress = db.DeliveryAddress.Where(n => (n.UserID == userId) && (n.DefaultAddress == true)).FirstOrDefault();
                if (userAddress != null)
                {
                    order.DeliveryAddress = userAddress.DeliveryAddress;
                    order.ReceiverCellphone = userAddress.ReceiverCellphone;
                }
                else
                {
                    order.DeliveryAddress = "***";
                    order.ReceiverCellphone = "***";
                }
                order.Sums = 0;
                order.State = (Int32)OrderState.WaitPrice;
                db.Orders.Add(order);
                db.SaveChanges();
                Submit submit = new Submit();
                var models = db.ModelPartsDetail.Where(n => n.ModelID == modelId);
                var model = db.ModelLibrary.Where(n => n.ModelID == modelId).FirstOrDefault();
                foreach (var item in models)
                {
                    var orderdetail = new OrderDetail();
                    orderdetail.OrderID = order.OrderID;
                    orderdetail.ModelID = modelId;
                    orderdetail.PrintParameter = "材料:" + material + ";" + "颜色:" + color + ";" + "打印精度:" + precision + ";" + "打印强度:" + fillRate + ";" + "缩放比例:" + optionalRatio + ";";
                    orderdetail.Counts = count;
                    orderdetail.Price = item.ModelPartPrintPrice;
                    orderdetail.ModelFileName = item.ModelFileName;
                    orderdetail.ModelName = model.ModelName;
                    orderdetail.Sums = orderdetail.Price * orderdetail.Counts;
                    //if print parameter is different from defaul paramter, create new price
                    bool differentParam = ((precision != "低") || (fillRate != "一般") || (optionalRatio != "1")) ? true : false;
                    if (differentParam)
                    {
                        //judge if the orderdetail with same modelId and parameter is existed, if not exist, create new print price
                        var isExist = db.OrderDetail.Where(n => (n.ModelID == modelId) && (n.PrintParameter == orderdetail.PrintParameter) && (n.ModelFileName == item.ModelFileName)).OrderByDescending(n => n.OrderID).FirstOrDefault();
                        if (isExist != null)
                        {
                            orderdetail.Price = isExist.Price;
                            orderdetail.Sums = orderdetail.Price * orderdetail.Counts;
                            orderdetail.State = (Int32)OrderState.WaitPay;
                            order.State = (Int32)OrderState.WaitSubmitOrder;
                            order.Sums = orderdetail.Sums;
                        }
                        else
                        {
                            orderdetail.State = (Int32)OrderState.WaitPrice;
                            order.State = (Int32)OrderState.WaitPrice;
                            order.Sums = 0;
                        }
                    }
                    else
                    {
                        orderdetail.State = (Int32)OrderState.WaitPay;
                        order.State = (Int32)OrderState.WaitSubmitOrder;
                        order.Sums = orderdetail.Sums;
                    }
                    db.Entry(order).State = EntityState.Modified;
                    db.OrderDetail.Add(orderdetail);
                    submit.orderdetail.Add(orderdetail);
                }
                db.SaveChanges();

                //2018.1.11新增
                float newFillRate;
                int fillRate2;
                switch (fillRate)
                {
                    case "很强": newFillRate = 0.4F;fillRate2 = 40; break;
                    case "强": newFillRate = 0.3F; fillRate2 = 30; break;
                    case "一般": newFillRate = 0.15F; fillRate2 = 15; break;
                    default:newFillRate = 0.3F; fillRate2 = 30; break;
                }
                int thickness;
                
                switch (precision)
                {
                    case "高": thickness = 100; break;
                    case "中": thickness = 150; break;
                    case "低": thickness = 200; break;
                    default: thickness = 200; break;
                }
                 
                var newModel = db.ModelPartsDetail.Where(n => n.ModelID == modelId).FirstOrDefault() ;
                OrderInfo orderInfo = new OrderInfo
                {
                    OrderCreatedTime = DateTime.Now.ToLocalTime(),
                    MaterialColor = color,
                    PrintPrecision = precision,
                    FillingRate = newFillRate,
                    ModelName = newModel.ModelFileName,
                    UserName = user.UserName,
                    ModelFilePath=newModel.ModelPath,
                    Ratio=1,
                    State=false
                };
                db.OrderInfo.Add(orderInfo);
                db.SaveChanges();
                var latestOrder = db.OrderInfo.OrderByDescending(n => n.OrderId).First();
                Models.OrderState orderState = new Models.OrderState
                {
                    OrderId=latestOrder.OrderId,
                    PrinterId=0,
                    PrintingCodeName="",
                    CurrentState="Created"
                };
                
                db.OrderState.Add(orderState);
                db.Configuration.ValidateOnSaveEnabled = false;
                db.SaveChanges();
                db.Configuration.ValidateOnSaveEnabled = true;
                string newModelPath = newModel.ModelPath;
                if (!Directory.Exists(Server.MapPath(newModelPath.Substring(0, newModelPath.LastIndexOf('/') + 1) + latestOrder.OrderId.ToString() + '/')))
                {
                    Directory.CreateDirectory(Server.MapPath(newModelPath.Substring(0, newModelPath.LastIndexOf('/') + 1) + latestOrder.OrderId.ToString() + '/'));
                }
                CreateCuraIni(Server.MapPath("/Files/cura.ini"), Server.MapPath(newModelPath.Substring(0,newModelPath.LastIndexOf('/')+1)+ latestOrder.OrderId.ToString()+ '/'+"cura.ini"), thickness, fillRate2);
                //calculate price
                //该模块尚未使用 
                var orderDetails = db.OrderDetail.Where(n => n.OrderID == orderId);
                foreach (var item in orderDetails)
                {
                    if (item.Price == 0)
                    {
                        orderParameter.OrderId = orderId;
                        orderParameter.ModelId = item.ModelID;
                        orderParameter.ModelFileName = item.ModelFileName;
                        var modelPath = db.ModelPartsDetail.Where(n => n.ModelID == item.ModelID && n.ModelFileName == item.ModelFileName).FirstOrDefault().ModelPath;
                        orderParameter.FilePath = Server.MapPath(modelPath);
                        orderParameter.LayerHeight = "0.01";
                        var parameters = item.PrintParameter.Split(';', ':');
                        orderParameter.FillRate = parameters[7];
                        orderParameter.Radio = parameters[9];
                        orderParameter.PrintParameter = item.PrintParameter;
                        CalculatePrice();
                    }
                }
                submit.order = order;
                var address = db.DeliveryAddress.Where(n => n.UserID == userId).ToArray();
                for (int i = 0; i < address.Count(); i++)
                {
                    submit.address.Add(address[i]);
                }
                //switch (order.State)
                //{
                //    case (Int32)OrderState.WaitPrice:
                //        return RedirectToAction("Order");
                //    case (Int32)OrderState.WaitSubmitOrder:
                //        return View("Submit", submit);
                //    default:
                //        return RedirectToAction("Order");
                //}
                return RedirectToAction("SubmitOrderSuccess");
            }
            else
            {
                return RedirectToAction("Login");
            }
        }
        public ActionResult SubmitOrderSuccess()
        {
            return View();
        }
        //读写cura.ini文件
        public void CreateCuraIni(string readPath,string writePath,int thickness,int fillRate)
        {
            int lineDistance = 175 * thickness / fillRate;
            List<string> para = new List<string> { "layerThickness", "filamentDiameter", "sparesInfillLineDistance" };
            try
            {
                using (StreamWriter sw = new StreamWriter(writePath))
                {

                    using (StreamReader sr = new StreamReader(readPath, Encoding.Default))
                    {
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            Console.WriteLine(line);
                            if (line.Contains(para[0]))
                                line = para[0] + "=" + thickness.ToString();
                            if (line.Contains(para[2]))
                                line = para[2] + "=" + lineDistance.ToString();
                            sw.WriteLine(line);
                        }
                    }
                }
            }
            catch (Exception e)
            { }
        }
        //开启新线程计算 模型预计打印价格
        public void CalculatePrice()
        {
            Thread t = new Thread(Threadfunc);
            t.Name = "ParameterThread";
            t.IsBackground = true;
            t.Start(orderParameter);
        }
        //thread for remote calculate order price 
        public void Threadfunc(object obj)
        {
            OrderParameter parameter = (OrderParameter)obj;
            // create the tcp connection
            TcpChannel channel = new TcpChannel();

            /*BlackBoxClass.BlackBoxClass boxClass = (BlackBoxClass.BlackBoxClass)Activator.GetObject(typeof(BlackBoxClass.BlackBoxClass), "tcp://localhost:8088/Hi");
            //call the function to calculate the time and consumption
            //Calculate model = obj.GetResult(fillRateS, layerHeightS, optionalRatioS, pathS);
            Calculate model = boxClass.GetResult(parameter.FillRate, parameter.LayerHeight, parameter.Radio, parameter.FilePath);*/
            //for local test
            //BlackBoxClass.BlackBoxClass boxClass = new BlackBoxClass.BlackBoxClass();
            //Calculate model = boxClass.GetResult(parameter.FillRate, parameter.LayerHeight, parameter.Radio, parameter.FilePath);

           // var singleModelPriceS = 2 * (0.7 * model.SumTime + 0.2 * model.TotalConsumption / 1000);
            //modify orderDetail state
            var orderDetail = db.OrderDetail.Where(n => n.ModelID == parameter.ModelId && n.OrderID == parameter.OrderId
                                                   && n.ModelFileName == parameter.ModelFileName && n.PrintParameter == parameter.PrintParameter).FirstOrDefault();
           // orderDetail.Price = Convert.ToDecimal(singleModelPriceS);
            orderDetail.Sums = orderDetail.Price * orderDetail.Counts;
            orderDetail.State = (Int32)OrderState.WaitSubmitOrder;
            db.SaveChanges();
            //modify order state
            var order = db.Orders.Find(orderDetail.OrderID);
            var orderDetails = db.OrderDetail.Where(n => n.OrderID == orderDetail.OrderID);
            foreach (var item in orderDetails)
            {
                if (item.Sums != 0)
                {
                    order.Sums += item.Sums;
                    order.State = (Int32)OrderState.WaitSubmitOrder;
                }
                else
                {
                    order.Sums = 0;
                    order.State = (Int32)OrderState.WaitPrice;
                    break;
                }
            }
            db.SaveChanges();
        }

        [HttpPost]
        public ActionResult AddPartOrder(FormCollection form)
        {
            if (Session["UserID"] != null)
            {
                ViewBag.UserId = Session["UserID"];
                int userId = Convert.ToInt32(Session["UserID"].ToString());
                var user = db.Users.Find(userId);
                ViewBag.UserName = user.UserName;

                string modelId = form["modelID"];
                string material = form["printMaterial"];
                string color = form["printColor"];
                string precision = form["printPrecision"];
                string fillRate = form["fillRate"];
                string optionalRatio = form["optionalRatio"];
                int count = Convert.ToInt32(form["buyCount"]);
                var partName = form["partName"];

                var order = new Orders();
                order.UserID = userId;
                Random random = new Random();
                order.OrderID = DateTime.Now.ToString("yyyyMMddHHmmss") + Convert.ToString(random.Next(1000, 9999));
                order.OrderTime = System.DateTime.Now;
                var userAddress = db.DeliveryAddress.Where(n => (n.UserID == userId) && (n.DefaultAddress == true)).FirstOrDefault();
                if (userAddress != null)
                {
                    order.DeliveryAddress = userAddress.DeliveryAddress;
                    order.ReceiverCellphone = userAddress.ReceiverCellphone;
                }
                else
                {
                    order.DeliveryAddress = "***";
                    order.ReceiverCellphone = "***";
                }
                order.Sums = 0;
                order.State = (Int32)OrderState.WaitPrice;
                db.Orders.Add(order);
                db.SaveChanges();

                Submit submit = new Submit();
                var models = db.ModelPartsDetail.Where(n => n.ModelID == modelId && n.ModelFileName == partName).FirstOrDefault();
                var model = db.ModelLibrary.Where(n => n.ModelID == modelId).FirstOrDefault();
                var orderdetail = new OrderDetail();
                orderdetail.OrderID = order.OrderID;
                orderdetail.ModelID = modelId;
                orderdetail.PrintParameter = "材料:" + material + ";" + "颜色:" + color + ";" + "打印精度:" + precision + ";" + "打印强度:" + fillRate + ";" + "缩放比例:" + optionalRatio + ";";
                orderdetail.Counts = count;
                orderdetail.Price = models.ModelPartPrintPrice;
                orderdetail.ModelFileName = models.ModelFileName;
                orderdetail.ModelName = model.ModelName;
                orderdetail.Sums = orderdetail.Price * orderdetail.Counts;
                //if print parameter is different from defaul paramter, create new price
                bool differentParam = ((precision != "低") || (fillRate != "一般") || (optionalRatio != "1")) ? true : false;
                if (differentParam)
                {
                    //judge if the orderdetail with same modelId and parameter is existed, if not exist, create new print price
                    var isExist = db.OrderDetail.Where(n => (n.ModelID == modelId) && (n.PrintParameter == orderdetail.PrintParameter) && (n.ModelFileName == models.ModelFileName)).OrderByDescending(n => n.OrderID).FirstOrDefault();
                    if (isExist != null)
                    {
                        orderdetail.Price = isExist.Price;
                        orderdetail.Sums = orderdetail.Price * orderdetail.Counts;
                        orderdetail.State = (Int32)OrderState.WaitPay;
                        order.State = (Int32)OrderState.WaitSubmitOrder;
                        order.Sums = orderdetail.Sums;
                    }
                    else
                    {
                        orderdetail.State = (Int32)OrderState.WaitPrice;
                        order.State = (Int32)OrderState.WaitPrice;
                        order.Sums = 0;
                    }
                }
                else
                {
                    orderdetail.State = (Int32)OrderState.WaitPay;
                    order.State = (Int32)OrderState.WaitSubmitOrder;
                    order.Sums = orderdetail.Sums;
                }
                db.Entry(order).State = EntityState.Modified;
                db.SaveChanges();
                db.OrderDetail.Add(orderdetail);
                db.SaveChanges();
                submit.orderdetail.Add(orderdetail);
                submit.order = order;
                var address = db.DeliveryAddress.Where(n => n.UserID == userId).ToArray();
                for (int i = 0; i < address.Count(); i++)
                {
                    submit.address.Add(address[i]);
                }
                switch (order.State)
                {
                    case (Int32)OrderState.WaitPrice:
                        return RedirectToAction("Order");
                    case (Int32)OrderState.WaitSubmitOrder:
                        return View("Submit", submit);
                    default:
                        return RedirectToAction("Order");
                }
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        //submit order ,wait for pay
        [HttpPost]
        public ActionResult SubmitOrder(FormCollection form)
        {
            string orderId = form["orderId"];
            string address = form["deliveryAddress"];
            var order = db.Orders.Where(n => n.OrderID == orderId).FirstOrDefault();
            if (address != null)
            {
                var deliveryAddress = db.DeliveryAddress.Where(n => n.DeliveryAddress == address).FirstOrDefault();
                order.DeliveryAddress = deliveryAddress.DeliveryAddress;
                order.ReceiverCellphone = deliveryAddress.ReceiverCellphone;
            }
            else
            {
                return JavaScript("alert(请创建收货地址！)");
            }
            order.State = (Int32)OrderState.WaitPay;
            db.Entry(order).State = EntityState.Modified;
            db.SaveChanges();
            //update orderdetail
            var orderdetail = db.OrderDetail.Where(n => n.OrderID == order.OrderID);
            foreach (var detail in orderdetail)
            {
                detail.State = (Int32)OrderState.WaitPay;
                db.Entry(detail).State = EntityState.Modified;
            }
            db.SaveChanges();
            return RedirectToAction("Order");
            //todo
            //redirect to pay page
        }
        //我的模型 详细信息
        public ActionResult UserModelDetail(string modelId)
        {
            //judge if user is logined
            if (Session["UserID"] != null)
            {
                ViewBag.UserId = Session["UserID"];
                int userId = Convert.ToInt32(Session["UserID"].ToString());
                var user = db.Users.Find(userId);
                ViewBag.UserName = user.UserName;

                var model = db.ModelLibrary.Where(n => (n.ModelID == modelId)).FirstOrDefault();
                Detail detail = new Detail();
                detail.DetailModel = model;
                detail.DetailModelID = modelId;
                var modelDetails = db.ModelPartsDetail.Where(n => n.ModelID == modelId);
                foreach (var item in modelDetails)
                {
                    detail.DetailModelParts.Add(item);
                }

                if (model.ModelPartCounts < 2)
                {
                    var modelDetail = db.ModelPartsDetail.Where(n => (n.ModelID == modelId) && (n.ModelFileName == model.ModelFileNames)).FirstOrDefault();
                    //calculate the optional ratio
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
            else
            {
                return RedirectToAction("Login");
            }
        }
        //用户评价
        public ActionResult Evaluation(string orderId, string modelId, string parameter)
        {
            if (Session["UserID"] != null)
            {
                ViewBag.UserId = Session["UserID"];
                int userId = Convert.ToInt32(Session["UserID"].ToString());
                var user = db.Users.Find(userId);
                ViewBag.UserName = user.UserName;

                var orderDetail = db.OrderDetail.Where(n => (n.OrderID == orderId) && (n.ModelID == modelId) && (n.PrintParameter == parameter)).FirstOrDefault();
                return View(orderDetail);
            }
            else
            {
                return RedirectToAction("Login");
            }
        }
        [HttpPost]
        public ActionResult SubmitEvaluation(FormCollection form)
        {
            if (Session["UserID"] != null)
            {
                string orderId = form["orderId"];
                string modelId = form["modelId"];
                string parameter = form["parameter"];
                string evaluation = (form["evaluation"] != null ? form["evaluation"].ToString() : "");
                decimal score = Convert.ToDecimal(form["score"]);

                if ((orderId == null) || (modelId == null) || (parameter == null))
                {
                    return JavaScript("alert(有错误，请重新提交！)");
                }
                var orderDetail = db.OrderDetail.Where(n => (n.OrderID == orderId) && (n.ModelID == modelId) && (n.PrintParameter == parameter)).FirstOrDefault();
                orderDetail.Evaluation = evaluation;
                db.Entry(orderDetail).State = EntityState.Modified;
                db.SaveChanges();
                var model = db.ModelLibrary.Where(n => n.ModelID == modelId).FirstOrDefault();
                model.ModelScores = (model.ModelScores + score) / ((decimal)2);
                db.Entry(model).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Order");
            }
            else
            {
                return RedirectToAction("Login");
            }
        }
        //搜索提示
        [HttpPost]
        public ActionResult SearchHints(string words)
        {
            if ((words == null) || (words == ""))
            {
                return Json(new { });
            }
            else
            {
                var models = db.ModelLibrary.Where(n => n.ModelKeyWords.Contains(words)).Select(r => new
                { label = r.ModelName });
                return Json(models);
            }
        }
    }
}
