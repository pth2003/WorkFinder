using Microsoft.AspNetCore.Mvc;

namespace WorkFinder.Web.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/{statusCode}")]
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

        [Route("Error")]
        public IActionResult Error()
        {
            return View();
        }

        [Route("NotFound")]
        public IActionResult NotFound()
        {
            return View();
        }
    }
}