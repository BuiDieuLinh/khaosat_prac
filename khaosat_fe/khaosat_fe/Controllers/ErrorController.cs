using Microsoft.AspNetCore.Mvc;

namespace khaosat_fe.Controllers
{
    public class ErrorController : Controller
    {
        [Route("401")]
        public IActionResult UnauthorizedPage()
        {
            Response.StatusCode = 401;
            return View("Unauthorized");
        }

        [Route("403")]
        public IActionResult Forbidden()
        {
            Response.StatusCode = 403;
            return View();
        }
    }
}
