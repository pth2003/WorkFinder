using Microsoft.AspNetCore.Mvc;

namespace WorkFinder.Web.Areas.Employer.Controllers
{
    [Area("Employer")]
    public class ErrorController : Controller
    {
        [Route("Employer/Error/{statusCode}")]
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

        [Route("Employer/Error")]
        public IActionResult Error()
        {
            return View();
        }

        [Route("Employer/NotFound")]
        public IActionResult NotFound()
        {
            return View("NotFound");
        }
    }
}