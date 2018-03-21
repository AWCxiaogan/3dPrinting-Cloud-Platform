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
    using model = Models.OrderState;
    public class OrderStateController : Controller
    {
        OnlinePrintDbContext db = new OnlinePrintDbContext();
        [HttpPost]
        public int Create(model state)
        {
            if (db.OrderState.Find(state.OrderId) != null)
                return -1;
            state.StartPrintingTime = DateTime.Now.ToLocalTime();
            db.OrderState.Add(state);
            db.SaveChanges();
            return 1;
        }
        [HttpPost]
        public int Delete(model state)
        {
            var query = db.OrderState.Find(state.OrderId);
            if (query == null)
                return -1;
            db.OrderState.Remove(query);
            db.SaveChanges();
            return 1;
        }
        [HttpPost]
        public int Update(model state)
        {
            var query = db.OrderState.Find(state.OrderId);
            if (query == null)
                return -1;
            query.PrinterId = state.PrinterId;
            query.PrintingCodeName= state.PrintingCodeName;
            query.PrintingLayer= state.PrintingLayer;
            query.PrintingFinishRatio= state.PrintingFinishRatio;
            query.PrintingTimeUsed= state.PrintingTimeUsed;
            query.PrintingTotalTime= state.PrintingTotalTime;
            query.StartPrintingTime= state.StartPrintingTime;
            db.SaveChanges();
            return 1;
        }
        [HttpPost]
        public JsonResult Read(model state)
        {
            db.Configuration.ProxyCreationEnabled = false;  //允许跨域访问
            var query = db.OrderState.Find(state.OrderId);
            if (query == null)
                return null;
            return Json(query);
        }
        [HttpPost]
        public JsonResult ReadAll()
        {
            db.Configuration.ProxyCreationEnabled = false;  //允许跨域访问
            IEnumerable<model> query = from q in db.OrderState
                                       select q;
            return Json(query.OrderBy(item => item.OrderId).Take(20));  //取最新的20条数据
        }
    }
}