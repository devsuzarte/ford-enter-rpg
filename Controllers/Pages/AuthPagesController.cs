using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace FordEnterRPG.Controllers.Pages
{
    public class SignInPageController : Controller
    {
        private readonly Services.IUserService _userService;
        public SignInPageController(Services.IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("/SignIn")]
        public IActionResult Index()
        {
            return View("~/Views/SignIn.cshtml");
        }

        [HttpPost("/SignIn")]
        public async Task<IActionResult> SignIn([FromForm] DTOs.UserSignInDto dto)
        {
            var user = await _userService.ValidateUserAsync(dto);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Credenciais inválidas");
                return View("~/Views/SignIn.cshtml");
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role ?? "User")
            };
            var identity = new ClaimsIdentity(claims, "Cookies");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("Cookies", principal);
            return Redirect("/Profile");
        }
    }

    public class SignOutPageController : Controller
    {
        [HttpPost("/SignOut")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            Response.Cookies.Delete("access_token");
            return Redirect("/SignIn");
        }
    }

    public class SignUpPageController : Controller
    {
        private readonly Services.IUserService _userService;
        public SignUpPageController(Services.IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("/SignUp")]
        public IActionResult Index()
        {
            return View("~/Views/SignUp.cshtml");
        }

        [HttpPost("/SignUp")]
        public async Task<IActionResult> SignUp([FromForm] DTOs.UserSignUpDto dto)
        {
            var result = await _userService.RegisterAsync(dto);
            if (!result)
            {
                ModelState.AddModelError(string.Empty, "Email já cadastrado.");
                return View("~/Views/SignUp.cshtml");
            }
            return Redirect("/SignIn");
        }
    }
}
