using FordEnterRPG.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FordEnterRPG.Controllers.Pages
{
    [Authorize]
    public class BattlePagesController : Controller
    {
        private readonly ICharacterService _characterService;
        private readonly IBattleService _battleService;

        public BattlePagesController(ICharacterService characterService, IBattleService battleService)
        {
            _characterService = characterService;
            _battleService = battleService;
        }

        [HttpGet("/Battle/Start")]
        public async Task<IActionResult> Start()
        {
            var userId = GetUserId();
            if (userId == 0) return Redirect("/SignIn");

            var character = await _characterService.GetByUserIdAsync(userId);
            if (character == null) return Redirect("/Character/Create");

            // Reuse an active battle if it exists
            var active = await _battleService.GetActiveBattleAsync(character.Id);
            if (active != null) return Redirect($"/Battle/{active.Id}");

            var battle = await _battleService.StartBattleAsync(character);
            return Redirect($"/Battle/{battle.Id}");
        }

        [HttpGet("/Battle/{id:int}")]
        public async Task<IActionResult> Show(int id)
        {
            var userId = GetUserId();
            if (userId == 0) return Redirect("/SignIn");

            var character = await _characterService.GetByUserIdAsync(userId);
            if (character == null) return Redirect("/Character/Create");

            var battle = await _battleService.GetBattleWithLogsAsync(id);
            if (battle == null || battle.CharacterId != character.Id)
                return Redirect("/Character");

            var full = await _characterService.GetByIdWithSkillsAsync(character.Id);
            ViewBag.Character = full;
            ViewBag.Battle = battle;
            return View("~/Views/Battle.cshtml");
        }

        [HttpPost("/Battle/{id:int}/Turn")]
        public async Task<IActionResult> Turn(int id, [FromForm] int skillId)
        {
            var userId = GetUserId();
            if (userId == 0) return Redirect("/SignIn");

            var base_ = await _characterService.GetByUserIdAsync(userId);
            if (base_ == null) return Redirect("/Character");
            var character = await _characterService.GetByIdWithSkillsAsync(base_.Id);
            if (character == null) return Redirect("/Character");

            var battle = await _battleService.GetBattleWithLogsAsync(id);
            if (battle == null || battle.CharacterId != character.Id || battle.Status != "Active")
                return Redirect("/Character");

            var skill = character.CharacterSkills.FirstOrDefault(cs => cs.SkillId == skillId)?.Skill;
            if (skill == null) return Redirect($"/Battle/{id}");

            await _battleService.ExecuteTurnAsync(battle, character, skill);
            return Redirect($"/Battle/{id}");
        }

        [HttpGet("/Battle/{id:int}/Reward")]
        public async Task<IActionResult> Reward(int id)
        {
            var userId = GetUserId();
            if (userId == 0) return Redirect("/SignIn");

            var character = await _characterService.GetByUserIdAsync(userId);
            if (character == null) return Redirect("/Character");

            var battle = await _battleService.GetBattleWithLogsAsync(id);
            if (battle == null || battle.CharacterId != character.Id || battle.Status != "Won" || battle.RewardClaimed)
                return Redirect("/Character");

            ViewBag.Battle = battle;
            ViewBag.Character = character;
            return View("~/Views/Reward.cshtml");
        }

        [HttpPost("/Battle/{id:int}/Reward")]
        public async Task<IActionResult> ClaimReward(int id, [FromForm] string choice)
        {
            var userId = GetUserId();
            if (userId == 0) return Redirect("/SignIn");

            var base_ = await _characterService.GetByUserIdAsync(userId);
            if (base_ == null) return Redirect("/Character");
            var character = await _characterService.GetByIdWithSkillsAsync(base_.Id);
            if (character == null) return Redirect("/Character");

            var battle = await _battleService.GetBattleWithLogsAsync(id);
            if (battle == null || battle.CharacterId != character.Id || battle.Status != "Won" || battle.RewardClaimed)
                return Redirect("/Character");

            var (replaced, acquired) = await _battleService.ClaimRewardAsync(battle, character, choice);

            if (choice == "Skill" && acquired != null)
            {
                TempData["AcquiredSkillName"]        = acquired.Name;
                TempData["AcquiredSkillRarity"]      = acquired.Rarity;
                TempData["AcquiredSkillBaseDamage"]  = acquired.BaseDamage.ToString();
                TempData["AcquiredSkillEffectType"]  = acquired.EffectType;
                TempData["AcquiredSkillEffectValue"] = acquired.EffectValue.ToString();
                TempData["AcquiredSkillDescription"] = acquired.Description;

                if (replaced != null)
                {
                    TempData["ReplacedSkillName"]        = replaced.Name;
                    TempData["ReplacedSkillRarity"]      = replaced.Rarity;
                    TempData["ReplacedSkillBaseDamage"]  = replaced.BaseDamage.ToString();
                    TempData["ReplacedSkillEffectType"]  = replaced.EffectType;
                    TempData["ReplacedSkillEffectValue"] = replaced.EffectValue.ToString();
                    TempData["ReplacedSkillDescription"] = replaced.Description;
                }

                return Redirect("/Battle/SkillSwap");
            }

            return Redirect("/Battle/Start");
        }

        [HttpGet("/Battle/SkillSwap")]
        public IActionResult SkillSwap()
        {
            if (TempData.Peek("AcquiredSkillName") == null)
                return Redirect("/Character");

            return View("~/Views/SkillSwap.cshtml");
        }

        private int GetUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(claim, out int id) ? id : 0;
        }
    }
}
