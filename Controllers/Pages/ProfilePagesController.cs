using FordEnterRPG.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace FordEnterRPG.Controllers.Pages
{
    [Authorize]
    public class ProfilePagesController : Controller
    {
        private readonly IUserService _userService;
        public ProfilePagesController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("/Profile")]
        public async Task<IActionResult> Index()
        {
            var email = User.Identity?.Name;
            if (string.IsNullOrEmpty(email))
                return Redirect("/SignIn");
            var profile = await _userService.GetProfileAsync(email);
            if (profile == null)
                return Redirect("/SignIn");
            return View("Profile", profile);
        }
    }
}
