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
    using model = StackerState;
    //public class WarehousePositionController : ApiController
    public class StackerStateController : Controller
    {
        OnlinePrintDbContext db = new OnlinePrintDbContext();
        [HttpPost]
        public int Create(model state)
        {
            state.CurrentTime = DateTime.Now.ToLocalTime();
            db.StackerState.Add(state);
            db.SaveChanges();
            return 1;
        }
        [HttpPost]
        public int Delete(model state)
        {
            var query = db.StackerState.Find(state.StateId);
            if (query == null)
                return -1;
            db.StackerState.Remove(query);
            db.SaveChanges();
            return 1;
        }
        //[HttpPost]
        //public int Update(model state)
        //{
        //    var query = db.StackerState.Find(state.CurrentTime);
        //    if (query == null)
        //        return -1;
        //    query.StackerForkSpeed = state.StackerForkSpeed;
        //    query.StackerLiftSpeed = state.StackerLiftSpeed;
        //    query.StackerMode = state.StackerMode;
        //    query.StackerWalkSpeed = state.StackerWalkSpeed;
        //    query.TheStackerState = state.TheStackerState;
        //    db.SaveChanges();
        //    return 1;
        //}
        [HttpPost]
        public JsonResult Read(model state)
        {
            db.Configuration.ProxyCreationEnabled = false;
            IEnumerable<model> query = from q in db.StackerState
                                       where q.StateId == state.StateId && state.CurrentTime.Day == q.CurrentTime.Day && q.CurrentTime.Month == state.CurrentTime.Month && q.CurrentTime.Year == state.CurrentTime.Year
                                       select q;
            return Json(query.OrderByDescending(item => item.StateId));
        }
        [HttpPost]
        public JsonResult ReadAll()
        {
            db.Configuration.ProxyCreationEnabled = false;
            IEnumerable<model> query = from q in db.StackerState
                                       select q;
            return Json(query.OrderByDescending(item => item.StateId).Take(20));  //取最新的20条数据
        }

    }
}