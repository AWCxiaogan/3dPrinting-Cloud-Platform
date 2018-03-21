using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
//using System.Web.Http;
using OnlinePrint.DomainLogic;
using System.Web.Mvc;
using OnlinePrint.Models;

namespace OnlinePrint.Controllers
{
    using model = UserInfo;
    //public class WarehousePositionController : ApiController
    public class UserInfoController : Controller
    {

        OnlinePrintDbContext db = new OnlinePrintDbContext();
        [HttpPost]
        public int Create(model state)
        {
            if (db.UserInfo.Find(state.UserId) != null)
                return -1;
            db.UserInfo.Add(state);
            db.SaveChanges();
            return 1;
        }
        [HttpPost]
        public int Delete(model state)
        {
            var query = db.UserInfo.Find(state.UserId);
            if (query == null)
                return -1;
            db.UserInfo.Remove(query);
            db.SaveChanges();
            return 1;
        }
        //[HttpPost]
        //public int Update(model state)
        //{
        //    var query = db.UserInfo.Find(state.UserId);
        //    if (query == null)
        //        return -1;
        //    query.UserId = state.UserId;
        //    query.ModelName=state.ModelName;
        //    query.ModelKeyWords = state.ModelKeyWords;
        //    query.ModelFileName = state.ModelFileName;
        //    query.ModelPhotoPath = state.ModelPhotoPath;
        //    query.ModelDownloadTimes = state.ModelDownloadTimes;
        //    query.ModelPrintTimes = state.ModelPrintTimes;
        //    query.ModelFilePath = state.ModelFilePath;
        //    query.ModelCreatedTime = state.ModelCreatedTime;
        //    db.SaveChanges();
        //    return 1;
        //}
        [HttpPost]
        public JsonResult Read(model state)
        {
            db.Configuration.ProxyCreationEnabled = false;  //允许跨域访问
            var query = db.UserInfo.Find(state.UserId);
            if (query == null)
                return null;
            return Json(query);
        }
        [HttpPost]
        public JsonResult ReadAll(int num=20)
        {
            db.Configuration.ProxyCreationEnabled = false;  //允许跨域访问
            IEnumerable<model> query = from q in db.UserInfo
                                       select q;
            return Json(query.OrderBy(item => item.UserId).Take(num));  //取最新的20条数据
        }
    }
}