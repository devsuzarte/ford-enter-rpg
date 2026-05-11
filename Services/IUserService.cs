using FordEnterRPG.DTOs;
using System.Threading.Tasks;

namespace FordEnterRPG.Services
{
    public interface IUserService
    {
        Task<bool> RegisterAsync(UserSignUpDto dto);
        Task<string?> AuthenticateAsync(UserSignInDto dto);
        Task<UserProfileDto?> GetProfileAsync(string email);
    }
}
