using FordEnterRPG.DTOs;
using FordEnterRPG.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FordEnterRPG.Controllers.Pages
{
    [Authorize]
    public class CharacterPagesController : Controller
    {
        private readonly ICharacterService _characterService;
        private readonly IBattleService _battleService;

        public CharacterPagesController(ICharacterService characterService, IBattleService battleService)
        {
            _characterService = characterService;
            _battleService = battleService;
        }

        [HttpGet("/Character")]
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            if (userId == 0) return Redirect("/SignIn");

            var character = await _characterService.GetByUserIdAsync(userId);
            if (character == null) return Redirect("/Character/Create");

            var full = await _characterService.GetByIdWithSkillsAsync(character.Id);

            // Check for unfinished/won battles
            var activeBattle = await _battleService.GetActiveBattleAsync(character.Id);
            ViewBag.ActiveBattle = activeBattle;

            return View("~/Views/CharacterDetail.cshtml", full);
        }

        [HttpGet("/Character/Create")]
        public async Task<IActionResult> Create()
        {
            var userId = GetUserId();
            if (userId == 0) return Redirect("/SignIn");

            // Prevent creating a second character
            var existing = await _characterService.GetByUserIdAsync(userId);
            if (existing != null) return Redirect("/Character");

            return View("~/Views/CharacterCreate.cshtml");
        }

        [HttpPost("/Character/Create")]
        public async Task<IActionResult> Create([FromForm] CreateCharacterDto dto)
        {
            var userId = GetUserId();
            if (userId == 0) return Redirect("/SignIn");

            var character = await _characterService.CreateAsync(userId, dto);
            if (character == null)
            {
                ViewBag.Error = "Classe inválida. Escolha Warrior, Mage ou Archer.";
                return View("~/Views/CharacterCreate.cshtml");
            }

            return Redirect("/Character");
        }

        private int GetUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(claim, out int id) ? id : 0;
        }
    }
}
