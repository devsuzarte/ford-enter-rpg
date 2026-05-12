using FordEnterRPG.DTOs;

namespace FordEnterRPG.Services
{
    public interface IUserService
    {
        Task<bool> RegisterAsync(UserSignUpDto dto);
        Task<string?> AuthenticateAsync(UserSignInDto dto);
        Task<Models.User?> ValidateUserAsync(UserSignInDto dto);
        Task<UserProfileDto?> GetProfileAsync(string email);
    }
}
