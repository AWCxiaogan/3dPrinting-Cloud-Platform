using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
//using System.Web.Http;
using OnlinePrint.DomainLogic;
using System.Web.Mvc;
using OnlinePrint.Models;
using OnlinePrint.Services;

namespace OnlinePrint.Controllers
{
    //using model = PrinterInfo;
    //public class WarehousePositionController : ApiController
    public class PrinterInfoController : Controller
    {
        private PrinterService _orderService = new PrinterService(new Repository<PrinterInfo>(new OnlinePrintDbContext()));
        
        //OnlinePrintDbContext db = new OnlinePrintDbContext();
        //[HttpPost]
        //public int Create(model state)
        //{
        //    if (db.PrinterInfo.Find(state.PrinterId) != null)
        //        return -1;
        //    db.PrinterInfo.Add(state);
        //    db.SaveChanges();
        //    return 1;
        //}
        //[HttpPost]
        //public int Delete(model state)
        //{
        //    var query = db.PrinterInfo.Find(state.PrinterId);
        //    if (query == null)
        //        return -1;
        //    db.PrinterInfo.Remove(query);
        //    db.SaveChanges();
        //    return 1;
        //}
        //[HttpPost]
        //public int Update(model state)
        //{
        //    var query = db.PrinterInfo.Find(state.PrinterId);
        //    if (query == null)
        //        return -1;
        //    query.PrinterType = state.PrinterType;
        //    query.PrinterRangeX = state.PrinterRangeX;
        //    query.PrinterRangeY = state.PrinterRangeY;
        //    query.PrinterRangeZ = state.PrinterRangeZ;
        //    query.PrinterPrecision = state.PrinterPrecision;
        //    db.SaveChanges();
        //    return 1;
        //}
        //[HttpPost]
        //public JsonResult Read(model state)
        //{
        //    db.Configuration.ProxyCreationEnabled = false;
        //    var query = db.PrinterInfo.Find(state.PrinterId);
        //    if (query == null)
        //        return null;
        //    return Json(query);
        //}
        //[HttpPost]
        //public JsonResult ReadAll()
        //{
        //    db.Configuration.ProxyCreationEnabled = false;  //允许跨域访问
        //    IEnumerable<model> query = from q in db.PrinterInfo
        //                               select q;
        //    return Json(query.OrderBy(item => item.PrinterId).Take(20));  //取最新的20条数据
        //}
        [HttpGet]
        [Route("api/GetPrinterAll")]
        public ActionResult GetPrinterAll()
        {
            var data = _orderService.GetPrinterAll();
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        //下面的都要改

        //[HttpGet]
        //[Route("api/GetPrinter")]
        //public ActionResult GetPrinter()
        //{
        //    var data = _orderService.GetOrderInfo();
        //    return Json(data, JsonRequestBehavior.AllowGet);
        //}
        //[HttpPost]
        //[Route("api/UpdatePrinter")]
        //public ActionResult UpdatePrinter(PrinterInfoDto orderState)
        //{
        //    var result = _orderService.UpdateOrderState(orderState);
        //    return Json(result);
        //}
        //[HttpPost]
        //[Route("api/UpdatePrinterState")]
        //public ActionResult UpdatePrinterState(PrinterStateDto orderState)
        //{
        //    var result = _orderService.UpdateOrderState(orderState);
        //    return Json(result);
        //}
    }
}