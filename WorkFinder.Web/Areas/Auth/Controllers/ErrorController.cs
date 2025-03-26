using Microsoft.AspNetCore.Mvc;

namespace WorkFinder.Web.Areas.Auth.Controllers
{
    [Area("Auth")]
    public class ErrorController : Controller
    {
        [Route("Auth/Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            switch (statusCode)
            {
                case 404:
                    return View("NotFound");
                default:
                    return View("Error");
            }
        }

        [Route("Auth/Error")]
        public IActionResult Error()
        {
            return View();
        }

        [Route("Auth/NotFound")]
        public IActionResult NotFound()
        {
            return View();
        }
    }
}