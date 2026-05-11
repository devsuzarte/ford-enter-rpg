using Microsoft.AspNetCore.Mvc;

namespace FordEnterRPG.Controllers.Pages
{
    public class SignInPageController : Controller
    {
        [HttpGet("/SignIn")]
        public IActionResult Index()
        {
            return View("~/Views/SignIn.cshtml");
        }
    }

    public class SignUpPageController : Controller
    {
        [HttpGet("/SignUp")]
        public IActionResult Index()
        {
            return View("~/Views/SignUp.cshtml");
        }
    }
}
