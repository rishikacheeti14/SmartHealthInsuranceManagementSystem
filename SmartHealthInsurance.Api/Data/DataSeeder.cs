using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartHealthInsurance.Api.Enums;
using SmartHealthInsurance.Api.Models;

namespace SmartHealthInsurance.Api.Data
{
    public static class DataSeeder
    {
        public static void SeedAdmin(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            context.Database.Migrate();

            var passwordHasher = new PasswordHasher<User>();
            
            var adminUser = context.Users.FirstOrDefault(u => u.Email == "admin@health.com");

            if (adminUser == null)
            {
                adminUser = new User
                {
                    Email = "admin@health.com",
                    FirstName = "System",
                    LastName = "Admin",
                    Role = UserRole.Admin,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, "Admin@123");
                context.Users.Add(adminUser);
            }
            else
            {
                adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, "Admin@123");
                

                if (adminUser.Role != UserRole.Admin) adminUser.Role = UserRole.Admin;
                

                if (string.IsNullOrEmpty(adminUser.FirstName)) adminUser.FirstName = "System";
                if (string.IsNullOrEmpty(adminUser.LastName)) adminUser.LastName = "Admin";

                context.Users.Update(adminUser);
            }

            context.SaveChanges();
        }

        public static void ClearAllDataExceptAdmin(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            context.Database.ExecuteSqlRaw("DELETE FROM Payments");
            
            context.Database.ExecuteSqlRaw("DELETE FROM Claims");

            context.Database.ExecuteSqlRaw("DELETE FROM TreatmentRecords");
  
            context.Database.ExecuteSqlRaw("DELETE FROM Policies");

            context.Database.ExecuteSqlRaw("DELETE FROM Hospitals");

            context.Database.ExecuteSqlRaw("DELETE FROM Notifications");

            context.Database.ExecuteSqlRaw("DELETE FROM Users WHERE Email != 'admin@health.com'");

            context.SaveChanges();
        }
    }
}
