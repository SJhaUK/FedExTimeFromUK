using System.Net;
using GXCalcTool.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace GXCalcTool.Controllers
{
    public class CalculatorController : Controller
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        public CalculatorController(IHostingEnvironment hostingEnvironment) => _hostingEnvironment = hostingEnvironment;

        // GET: /Calculator/Load
        public IActionResult Load() => View();


        // GET: /Calculator/GxCalcPartial
        public IActionResult GxCalcPartial()
        {
            var model = new GxCalcMainModelContext(_hostingEnvironment).Model();

            return model != null
                ? (IActionResult) PartialView("GxCalcPartial", model)
                : Content(HttpStatusCode.InternalServerError.ToString());
        }


        // GET: /Calculator/GetRegionSuggestions?countryId=2&&keyword=jal
        public IActionResult GetRegionSuggestions(int countryId, string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return Content(HttpStatusCode.BadRequest.ToString());

            var model = new GxCalcMainModelContext(_hostingEnvironment).GetRegion(countryId, keyword);
            return Content(model ?? HttpStatusCode.InternalServerError.ToString());
        }


        // GET: /Calculator/GetDepotSuggestions?postcode=mk65lb
        public IActionResult GetDepotSuggestions(string postcode)
        {
            if (string.IsNullOrWhiteSpace(postcode) || postcode.Length < 6)
                return Content(HttpStatusCode.BadRequest.ToString());

            var model = new GxCalcMainModelContext(_hostingEnvironment).GetDepotSuggestion(postcode);
            return Content(model ?? HttpStatusCode.InternalServerError.ToString());
        }


        // GET: /Calculator/GetDepotSuggestions?postcode=mk65lb
        public IActionResult GetFdexTimeLine(int areaId)
        {
            if (areaId < 1)
                return Content(HttpStatusCode.BadRequest.ToString());

            var model = new GxCalcMainModelContext(_hostingEnvironment).GetFedExTimeLine(areaId);
            return Content(model ?? HttpStatusCode.InternalServerError.ToString());
        }

    }
}
