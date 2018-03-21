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
    using model = WarehouseState;
    //public class WarehousePositionController : ApiController
    public class WarehouseStateController : Controller
    {
        OnlinePrintDbContext db = new OnlinePrintDbContext();
        [HttpPost]
        //public int Create(int id,int state=0)
        public int Create(model state)
        {
            state.CurrentTime = DateTime.Now.ToLocalTime();
            db.WarehouseState.Add(state);
            db.SaveChanges();
            return 1;
        }
        [HttpPost]
        public int Delete(model state)
        {
            var query = db.WarehouseState.Find(state.StateId);
            if (query == null)
                return -1;
            db.WarehouseState.Remove(query);
            db.SaveChanges();
            return 1;
        }
        //[HttpPost]
        //public int Update(model state)
        //{
        //    var query = db.WarehouseState.Find(state.CurrentTime);
        //    if (query == null)
        //        return -1;
        //    query.WarehouseBoardNumber = state.WarehouseBoardNumber;
        //    query.WarehouseProductNumber = state.WarehouseProductNumber;
        //    query.WarehouseVacancyNumber = state.WarehouseVacancyNumber;
        //    query.StoragePlatformState = state.StoragePlatformState;
        //    query.OutboundPlatformState = state.OutboundPlatformState;
        //    query.ManualInterfaceState = state.ManualInterfaceState;
        //    db.SaveChanges();
        //    return 1;
        //}
        [HttpPost]
        public JsonResult Read(model state)
        {
            db.Configuration.ProxyCreationEnabled = false;
            IEnumerable<model> query = from q in db.WarehouseState
                                       where q.StateId == state.StateId && state.CurrentTime.Day == q.CurrentTime.Day && q.CurrentTime.Month == state.CurrentTime.Month && q.CurrentTime.Year == state.CurrentTime.Year
                                       select q;
            return Json(query.OrderByDescending(item => item.StateId));
        }
        [HttpPost]
        public JsonResult ReadAll()
        {
            db.Configuration.ProxyCreationEnabled = false;
            IEnumerable<model> query = from q in db.WarehouseState
                                       select q;
            return Json(query.OrderByDescending(item => item.StateId).Take(20));  //取最新的20条数据
        }

    }
}