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
    using model = WarehousePositionState;
    //public class WarehousePositionController : ApiController
    public class WarehousePositionStateController : Controller
    {
        OnlinePrintDbContext db = new OnlinePrintDbContext();
        readonly ILogic<model> logic = new WarehousePositionLogic();
        public WarehousePositionStateController()
        {
        }
        /// <summary>
        /// 添加库位条目
        /// </summary>
        /// <param name="id">库位编号</param>
        /// <param name="state">库位状态</param>
        /// <returns>1:添加成功，2：添加失败</returns>
        [HttpPost]
        //public int Create(int id,int state=0)
        public int Create(model state)
        {
            //state.WarehousePositionId = 5;
            return logic.Create(state);
        }
        [HttpPost]
        public int Delete(model state)
        {
            return logic.Delete(state);
        }
        [HttpPost]
        public int Update(model state)
        {
            return logic.Update(state);
        }
        [HttpPost]
        public JsonResult Read(model state)
        {
            return Json(logic.Read(state));
        }
        [HttpPost]
        public JsonResult ReadAll()
        {
            db.Configuration.ProxyCreationEnabled = false;  //允许跨域访问
            IEnumerable<model> query = from q in db.WarehousePositionState
                                       select q;
            return Json(query.OrderBy(item => item.WarehousePositionId).Take(20));  //取最新的20条数据
        }
    }
}