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
    using model = PrinterState;
    //public class WarehousePositionController : ApiController
    public class PrinterStateController : Controller
    {

        OnlinePrintDbContext db = new OnlinePrintDbContext();
        [HttpPost]
        public int Create(model state)
        {
            if (db.PrinterState.Find(state.StateId) != null)
                return -1;
            state.CurrentTime = DateTime.Now.ToLocalTime();
            db.PrinterState.Add(state);
            db.SaveChanges();
            return 1;
        }
        [HttpPost]
        public int Delete(model state)
        {
            var query = db.PrinterState.Find(state.StateId);
            if (query == null)
                return -1;
            db.PrinterState.Remove(query);
            db.SaveChanges();
            return 1;
        }
        //[HttpPost]
        //public int Update(model state)
        //{
        //    var query = db.PrinterState.Find(state.PrinterId,state.CurrentTime);
        //    if (query == null)
        //        return -1;
        //    query.PrinterInformation = state.PrinterInformation;
        //    query.PrintSpeed= state.PrintSpeed;
        //    query.WireFeedSpeed= state.WireFeedSpeed;
        //    query.PrintHeadTemp= state.PrintHeadTemp;
        //    query.FloorTemp= state.FloorTemp;
        //    query.CurrentTime= state.CurrentTime;
        //    db.SaveChanges();
        //    return 1;
        //}

        [HttpPost]
        public JsonResult ReadAll()
        {
            db.Configuration.ProxyCreationEnabled = false;
            IEnumerable<model> query = from q in db.PrinterState
                                       select q;
            return Json(query.OrderByDescending(item => item.StateId).Take(20));  //取最新的20条数据
        }
        /// <summary>
        /// 需同时给出Id和日期参数
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Read(model state)
        {
            db.Configuration.ProxyCreationEnabled = false;
            IEnumerable<model> query = from q in db.PrinterState
                                       where q.PrinterId == state.PrinterId && state.CurrentTime.Day == q.CurrentTime.Day && q.CurrentTime.Month == state.CurrentTime.Month && q.CurrentTime.Year == state.CurrentTime.Year
                                       select q;
            return Json(query.OrderByDescending(item => item.StateId));
        }

    }

}