using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
//using System.Web.Http;
using System.Web.Mvc;
using OnlinePrint.Models;
using OnlinePrint.ModelDto;
using OnlinePrint.Services;
using Newtonsoft.Json;

namespace OnlinePrint.Controllers
{

    //using model = OrderInfo;
    //public class WarehousePositionController : ApiController
    public class OrderInfoController : Controller
    {

        //OnlinePrintDbContext db = new OnlinePrintDbContext();


        private OrderService _orderService = new OrderService(new Repository<OrderInfo>(new OnlinePrintDbContext()));

        //[HttpPost]
        //public int Create(model state)
        //{
        //    if (db.OrderInfo.Find(state.OrderId) != null)
        //        return -1;
        //    state.OrderCreatedTime = DateTime.Now.ToLocalTime();
        //    db.OrderInfo.Add(state);
        //    db.SaveChanges();
        //    return 1;
        //}
        //[HttpPost]
        //public int Delete(model state)
        //{
        //    var query = db.OrderInfo.Find(state.OrderId);
        //    if (query == null)
        //        return -1;
        //    db.OrderInfo.Remove(query);
        //    db.SaveChanges();
        //    return 1;
        //}
        //[HttpPost]
        //public int Update(model state)
        //{
        //    var query = db.OrderInfo.Find(state.OrderId);
        //    if (query == null)
        //        return -1;
        //    query.ModelId = state.ModelId;
        //    query.UserId= state.UserId;
        //    query.OrderCreatedTime = state.OrderCreatedTime;
        //    query.OrderState = state.OrderState;
        //    query.MaterialColor = state.MaterialColor;
        //    query.PrintPrecision = state.PrintPrecision;
        //    query.FillingRate= state.FillingRate;
        //    query.WarehousePositionId= state.WarehousePositionId;
        //    db.SaveChanges();
        //    return 1;
        //}
        //[HttpPost]
        //public JsonResult Read(model state)
        //{
        //    db.Configuration.ProxyCreationEnabled = false;  //允许跨域访问
        //    var query = db.OrderInfo.Find(state.OrderId);
        //    if (query == null)
        //        return null;
        //    return Json(query);
        //}
        //[HttpPost]
        //public JsonResult ReadAll()
        //{
        //    db.Configuration.ProxyCreationEnabled = false;  //允许跨域访问
        //    IEnumerable<model> query = from q in db.OrderInfo
        //                               select q;
        //    return Json(query.OrderBy(item => item.OrderId).Take(20));  //取最新的20条数据
        //}
        [HttpGet]
        [Route("api/GetOrderAll")]
        public ActionResult GetOrderAll()
        {
            var data = _orderService.GetOrderAll();
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Route("api/GetOrder")]
        public ActionResult GetOrder()
        {
            var data = _orderService.GetOrderInfo();
            return Json(data,JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Route("api/UpdateOrderState")]
        public ActionResult UpdateOrderState(OrderStateDto orderState)
        {
            var result = _orderService.UpdateOrderState(orderState);
            return Json(result);
        }
        [HttpGet]
        [Route("api/FinishOrder")]
        public ActionResult FinishOrder(int orderId)
        {
            var data = _orderService.OrderIsFinish(orderId);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        [Route("api/CancelOrder")]
        public ActionResult CancelOrder(int orderId)
        {
            var data = _orderService.CancelOrder(orderId);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        [Route("api/GetOrderState")]
        public JsonResult GetOrderState(int orderId)
        {
            var data = _orderService.GetOrderState(orderId);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}