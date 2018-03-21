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
    using model = PrintTime;
    //public class WarehousePositionController : ApiController
    public class PrintTimeController : Controller
    {

        OnlinePrintDbContext db = new OnlinePrintDbContext();
        [HttpPost]
        public int Create(model state)
        {
            //if (db.PrintTime.Find(state.PrintTimeId) != null)
            //    return -1;
            state.StartPrintTime = DateTime.Now.ToLocalTime();  //时间状态在创建数据条目时自动生成
            db.PrintTime.Add(state);
            db.SaveChanges();
            return 1;
        }
        [HttpPost]
        public int Delete(model state)
        {
            var query = db.PrintTime.Find(state.StateId);
            if (query == null)
                return -1;
            db.PrintTime.Remove(query);
            db.SaveChanges();
            return 1;
        }
        [HttpPost]
        public int Update(model state)
        {
            var query = db.PrintTime.Find(state.StateId);
            if (query == null)
                return -1;
            query.FinishPrintTime = state.FinishPrintTime;
            db.SaveChanges();
            return 1;
        }
        [HttpPost]
        public JsonResult ReadAll()
        {
            db.Configuration.ProxyCreationEnabled = false;
            IEnumerable<model> query = from q in db.PrintTime
                        select q;
            return Json(query.OrderByDescending(item=>item.StateId).Take(20));  //取最新的20条数据
        }
        /// <summary>
        /// 读 所选Id的当天的状态信息
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Read(model state)
        {
            db.Configuration.ProxyCreationEnabled = false;
            IEnumerable<model> query = from q in db.PrintTime
                        where q.PrinterId == state.PrinterId && state.StartPrintTime.Day == q.StartPrintTime.Day && q.StartPrintTime.Month == state.StartPrintTime.Month && q.StartPrintTime.Year==state.StartPrintTime.Year
                        select q;
            return Json(query.OrderByDescending(item => item.StateId));
        }

    }
}