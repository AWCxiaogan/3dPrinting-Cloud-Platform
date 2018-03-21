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
    using model = ConveyingRobotState;
    //public class WarehousePositionController : ApiController
    public class ConveyingRobotStateController : Controller
    {

        OnlinePrintDbContext db = new OnlinePrintDbContext();
        [HttpPost]
        public int Create(model state)
        {
            state.CurrentTime = DateTime.Now.ToLocalTime();
            db.ConveyingRobotState.Add(state);
            db.SaveChanges();
            return 1;
        }
        [HttpPost]
        public int Delete(model state)
        {
            var query = db.ConveyingRobotState.Find(state.StateId);
            if (query == null)
                return -1;
            db.ConveyingRobotState.Remove(query);
            db.SaveChanges();
            return 1;
        }
        //[HttpPost]
        //public int Update(model state)
        //{
        //    var query = db.ConveyingRobotState.Find(state.CurrentTime);
        //    if (query == null)
        //        return -1;
        //    query.RobotState = state.RobotState;
        //    query.J1Position = state.J1Position;
        //    query.J2Position = state.J2Position;
        //    query.J3Position = state.J3Position;
        //    query.J4Position = state.J4Position;
        //    query.J5Position = state.J5Position;
        //    query.J6Position = state.J6Position;
        //    query.RobotPosition = state.RobotPosition;
        //    db.SaveChanges();
        //    return 1;
        //}
        [HttpPost]
        public JsonResult Read(model state)
        {
            db.Configuration.ProxyCreationEnabled = false;
            IEnumerable<model> query = from q in db.ConveyingRobotState
                                       where q.StateId == state.StateId && state.CurrentTime.Day == q.CurrentTime.Day && q.CurrentTime.Month == state.CurrentTime.Month && q.CurrentTime.Year == state.CurrentTime.Year
                                       select q;
            return Json(query.OrderByDescending(item => item.StateId));
        }
        [HttpPost]
        public JsonResult ReadAll()
        {
            db.Configuration.ProxyCreationEnabled = false;
            IEnumerable<model> query = from q in db.ConveyingRobotState
                                       select q;
            return Json(query.OrderByDescending(item => item.StateId).Take(20));  //取最新的20条数据
        }
    }
}