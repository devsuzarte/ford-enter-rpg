using FordEnterRPG.Data;
using FordEnterRPG.Models;
using Microsoft.EntityFrameworkCore;

namespace FordEnterRPG.Utils
{
    public static class AdminSeeder
    {
        public static async Task SeedAdminAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await context.Database.MigrateAsync();

            var adminEmail = "admin@admin.com";
            if (!await context.Users.AnyAsync(u => u.Email == adminEmail))
            {
                var admin = new User
                {
                    Name = "Admin",
                    Email = adminEmail,
                    PasswordHash = "admin", // Troque para hash seguro depois
                    Role = "Admin"
                };
                context.Users.Add(admin);
                await context.SaveChangesAsync();
            }
        }
    }
}
