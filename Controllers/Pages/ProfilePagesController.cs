using FordEnterRPG.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FordEnterRPG.Controllers.Pages
{
    [Authorize]
    public class ProfilePagesController : Controller
    {
        private readonly IUserService _userService;
        private readonly ICharacterService _characterService;

        public ProfilePagesController(IUserService userService, ICharacterService characterService)
        {
            _userService = userService;
            _characterService = characterService;
        }

        [HttpGet("/Profile")]
        public async Task<IActionResult> Index()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
                return Redirect("/SignIn");

            var profile = await _userService.GetProfileAsync(email);
            if (profile == null)
                return Redirect("/SignIn");

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim, out int userId))
            {
                var character = await _characterService.GetByUserIdAsync(userId);
                ViewBag.HasCharacter = character != null;
                ViewBag.CharacterId = character?.Id ?? 0;
            }

            return View("~/Views/Profile.cshtml", profile);
        }
    }
}
