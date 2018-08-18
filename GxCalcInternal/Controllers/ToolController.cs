using GXCalcTool.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;


namespace GXCalcWidget.Controllers
{
    public class ToolController : Controller
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        public ToolController(IHostingEnvironment hostingEnvironment) => _hostingEnvironment = hostingEnvironment;

        public ActionResult Load() => View();


        // GET: /Calculator/GxCalcPartial
        public PartialViewResult GxCalcTool()
        {
            var model = new GxCalcMainModelContext(_hostingEnvironment).Model();

            if (model != null)
            {
                return PartialView("~/Views/Tool/GxCalcTool.cshtml", model);
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

            var model = new GxCalcMainModelContext(_hostingEnvironment).GetRegion(countryId, keyword);
            return Content(model ?? HttpStatusCode.InternalServerError.ToString());
        }


        // GET: /Calculator/GetDepotSuggestions?postcode=mk65lb
        public ActionResult GetDepotSuggestions(string postcode)
        {
            if (string.IsNullOrWhiteSpace(postcode) || postcode.Length < 6)
                return Content(HttpStatusCode.BadRequest.ToString());

            postcode = Regex.Replace(postcode, @"\s+", "");
            postcode = postcode.ToLower();

            var model = new GxCalcMainModelContext(_hostingEnvironment).GetFedExTimeLine(postcode);
            return Content(model ?? HttpStatusCode.InternalServerError.ToString());
        }


        // GET: /Calculator/GetDepotSuggestions?areaId=1
        public ActionResult GetFdexTimeLine(int areaId, string postDate)
        {
            if (areaId < 1)
                return Content(HttpStatusCode.BadRequest.ToString());

            var model = new GxCalcMainModelContext(_hostingEnvironment).GetFedExTimeLine(areaId, postDate);
            return Content(model ?? HttpStatusCode.InternalServerError.ToString());
        }
    }

}
