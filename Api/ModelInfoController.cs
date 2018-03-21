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
    using model = ModelInfo;
    //public class WarehousePositionController : ApiController
    public class ModelInfoController : Controller
    {

        OnlinePrintDbContext db = new OnlinePrintDbContext();
        [HttpPost]
        public int Create(model state)
        {
            if (db.ModelInfo.Find(state.ModelId) != null)
                return -1;
            state.ModelCreatedTime = DateTime.Now.ToLocalTime();
            db.ModelInfo.Add(state);
            db.SaveChanges();
            return 1;
        }
        [HttpPost]
        public int Delete(model state)
        {
            var query = db.ModelInfo.Find(state.ModelId);
            if (query == null)
                return -1;
            db.ModelInfo.Remove(query);
            db.SaveChanges();
            return 1;
        }
        [HttpPost]
        public int Update(model state)
        {
            var query = db.ModelInfo.Find(state.ModelId);
            if (query == null)
                return -1;
            query.UserId = state.UserId;
            query.ModelName=state.ModelName;
            query.ModelKeyWords = state.ModelKeyWords;
            query.ModelFileName = state.ModelFileName;
            query.ModelPhotoPath = state.ModelPhotoPath;
            query.ModelDownloadTimes = state.ModelDownloadTimes;
            query.ModelPrintTimes = state.ModelPrintTimes;
            query.ModelFilePath = state.ModelFilePath;
            query.ModelCreatedTime = state.ModelCreatedTime;
            db.SaveChanges();
            return 1;
        }
        [HttpPost]
        public JsonResult Read(model state)
        {
            db.Configuration.ProxyCreationEnabled = false;  //允许跨域访问
            var query = db.ModelInfo.Find(state.ModelId);
            if (query == null)
                return null;
            return Json(query);
        }
        [HttpPost]
        public JsonResult ReadAll()
        {
            db.Configuration.ProxyCreationEnabled = false;  //允许跨域访问
            IEnumerable<model> query = db.ModelInfo.OrderBy(p => p.ModelId).Take(20);
            return Json(query.OrderBy(item => item.ModelId).Take(20));  //取最新的20条数据
        }
    }
}