using FordEnterRPG.Models;
using System.Threading.Tasks;

namespace FordEnterRPG.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task AddAsync(User user);
        Task<bool> EmailExistsAsync(string email);
    }
}
