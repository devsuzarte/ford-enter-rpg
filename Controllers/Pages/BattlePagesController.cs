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

            if (character.IsDead) return Redirect("/Character");

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

            if (character.IsDead && battle.Status == "Active")
                return Redirect("/Character");

            var full = await _characterService.GetByIdWithSkillsAsync(character.Id);
            ViewBag.Character = full;
            ViewBag.Battle = battle;

            /* pass attack results from the latest log so JS can animate them */
            if (battle.TurnCount > 0 && battle.Logs.Any() && battle.Status == "Active")
            {
                var lastLog = battle.Logs.OrderByDescending(l => l.Turn).First();

                var enemy = ParseEnemyAttack(lastLog.Description, full!.Name);
                if (enemy.HasValue)
                {
                    ViewBag.EnemySlotTurn  = battle.TurnCount;
                    ViewBag.EnemySlotHit   = enemy.Value.hit;
                    ViewBag.EnemySlotSkill = enemy.Value.skill;
                }

                /* when enemy goes first the player slot must replay on page reload too */
                if (!battle.PlayerGoesFirst)
                {
                    var player = ParsePlayerAttack(lastLog.Description, full!.Name);
                    if (player.HasValue)
                    {
                        ViewBag.PlayerSlotHit   = player.Value.hit;
                        ViewBag.PlayerSlotSkill = player.Value.skill;
                    }
                }
            }

            return View("~/Views/Battle.cshtml");
        }

        [HttpPost("/Battle/{id:int}/Turn")]
        public async Task<IActionResult> Turn(int id, [FromForm] int skillId, [FromForm] string? isHit, [FromForm] string? playerGoesFirst)
        {
            var userId = GetUserId();
            if (userId == 0) return Redirect("/SignIn");

            var base_ = await _characterService.GetByUserIdAsync(userId);
            if (base_ == null) return Redirect("/Character");
            var character = await _characterService.GetByIdWithSkillsAsync(base_.Id);
            if (character == null) return Redirect("/Character");

            var battle = await _battleService.GetBattleWithLogsAsync(id);
            if (battle == null || battle.CharacterId != character.Id || battle.Status != "Active" || character.IsDead)
                return Redirect("/Character");

            var skill = character.CharacterSkills.FirstOrDefault(cs => cs.SkillId == skillId)?.Skill;
            if (skill == null) return Redirect($"/Battle/{id}");

            bool? clientIsHit = isHit != null ? isHit == "true" : null;
            bool? pgf = playerGoesFirst != null ? playerGoesFirst == "true" : null;
            await _battleService.ExecuteTurnAsync(battle, character, skill, clientIsHit, pgf);
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
        public async Task<IActionResult> ClaimReward(int id, [FromForm] string choice, [FromForm] string? rarity)
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

            var (replaced, acquired) = await _battleService.ClaimRewardAsync(battle, character, choice, rarity);

            if (choice == "Skill" && acquired != null)
            {
                TempData["AcquiredSkillName"]        = acquired.Name;
                TempData["AcquiredSkillRarity"]      = acquired.Rarity;
                TempData["AcquiredSkillClassName"]   = acquired.ClassName;
                TempData["AcquiredSkillBaseDamage"]  = acquired.BaseDamage.ToString();
                TempData["AcquiredSkillEffectType"]  = acquired.EffectType;
                TempData["AcquiredSkillEffectValue"] = acquired.EffectValue.ToString();
                TempData["AcquiredSkillDescription"] = acquired.Description;

                if (replaced != null)
                {
                    TempData["ReplacedSkillName"]        = replaced.Name;
                    TempData["ReplacedSkillRarity"]      = replaced.Rarity;
                    TempData["ReplacedSkillClassName"]   = replaced.ClassName;
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

        private static (bool hit, string skill)? ParseEnemyAttack(string desc, string playerName)
        {
            string? skill = null;
            bool? hit = null;

            foreach (var raw in desc.Split('\n'))
            {
                var ln = raw.Trim();
                if (ln.StartsWith("ATAQUE: ") && ln.Contains(" usa '"))
                {
                    var actorEnd = ln.IndexOf(" usa '");
                    var actor = ln[8..actorEnd];
                    if (actor != playerName)
                    {
                        var si = ln.IndexOf("usa '") + 5;
                        var ei = ln.IndexOf("'", si);
                        if (ei > si) skill = ln[si..ei];
                        hit = null;
                    }
                }
                else if (skill != null && hit == null)
                {
                    if (ln.Contains("[ERROU]")) hit = false;
                    else if (ln.StartsWith("Dano causado:")) hit = true;
                }
            }

            if (skill != null && hit.HasValue)
                return (hit.Value, skill);
            return null;
        }

        private static (bool hit, string skill)? ParsePlayerAttack(string desc, string playerName)
        {
            bool nextIsResult = false;
            string playerSkill = "";

            foreach (var raw in desc.Split('\n'))
            {
                var ln = raw.Trim();
                if (string.IsNullOrEmpty(ln) || ln.StartsWith("===")) continue;

                if (nextIsResult)
                {
                    if (ln.Contains("[ERROU]"))       return (false, playerSkill);
                    if (ln.StartsWith("Dano causado:")) return (true,  playerSkill);
                    nextIsResult = false;
                }

                if (ln.StartsWith("ATAQUE: ") && ln.Contains(" usa '"))
                {
                    var actorEnd = ln.IndexOf(" usa '");
                    var actor    = ln[8..actorEnd];
                    if (actor == playerName)
                    {
                        var si = ln.IndexOf("usa '") + 5;
                        var ei = ln.IndexOf("'", si);
                        if (ei > si) { playerSkill = ln[si..ei]; nextIsResult = true; }
                    }
                }
            }

            return null;
        }

        private int GetUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(claim, out int id) ? id : 0;
        }
    }
}
