using System.ComponentModel.DataAnnotations;

namespace FordEnterRPG.DTOs
{
    public class UserSignUpDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        [MinLength(6, ErrorMessage = "A senha deve ter pelo menos 6 caracteres.")]
        public string Password { get; set; } = string.Empty;
    }
}
