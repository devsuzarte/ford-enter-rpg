using FordEnterRPG.DTOs;
using FordEnterRPG.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading.Tasks;

namespace FordEnterRPG.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("SignUp")]
        [SwaggerResponse(200, "Usuário cadastrado com sucesso")]
        [SwaggerResponse(400, "Email já cadastrado")]
        public async Task<IActionResult> SignUp([FromForm] UserSignUpDto dto)
        {
            var result = await _userService.RegisterAsync(dto);
            if (!result) return BadRequest("Email já cadastrado.");
            return Ok();
        }

        [HttpPost("SignIn")]
        [SwaggerResponse(200, "Autenticado com sucesso", typeof(string))]
        [SwaggerResponse(401, "Credenciais inválidas")]
        public async Task<IActionResult> SignIn([FromForm] UserSignInDto dto)
        {
            var token = await _userService.AuthenticateAsync(dto);
            if (token == null) return Unauthorized();
            return Ok(new { token });
        }
    }
}
