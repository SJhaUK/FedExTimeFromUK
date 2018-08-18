
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using GXCalcTool.Models;

namespace GXCalcWidget.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        // GET: /Calculator/Load
        public ActionResult Load() => View();


        // GET: /Calculator/GxCalcPartial
        public PartialViewResult GxCalcPartial()
        {
            var model = new GxCalcMainModelContext().Model();

            if (model != null)
            {
                return PartialView("GxCalcPartial", model);
            }
            else
            {
                return new PartialViewResult(); 
            }
                
        }


        // GET: /Calculator/GetRegionSuggestions?countryId=2&&keyword=jal
        public ActionResult GetRegionSuggestions(int countryId, string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return Content(HttpStatusCode.BadRequest.ToString());

            var model = new GxCalcMainModelContext().GetRegion(countryId, keyword);
            return Content(model ?? HttpStatusCode.InternalServerError.ToString());
        }


        // GET: /Calculator/GetDepotSuggestions?postcode=mk65lb
        public ActionResult GetDepotSuggestions(string postcode)
        {
            if (string.IsNullOrWhiteSpace(postcode) || postcode.Length < 6)
                return Content(HttpStatusCode.BadRequest.ToString());

            var model = new GxCalcMainModelContext().GetDepotSuggestion(postcode);
            return Content(model ?? HttpStatusCode.InternalServerError.ToString());
        }


        // GET: /Calculator/GetDepotSuggestions?postcode=mk65lb
        public ActionResult GetFdexTimeLine(int areaId, string postDate)
        {
            if (areaId < 1)
                return Content(HttpStatusCode.BadRequest.ToString());

            var model = new GxCalcMainModelContext().GetFedExTimeLine(areaId, postDate);
            return Content(model ?? HttpStatusCode.InternalServerError.ToString());
        }
    }
    
}