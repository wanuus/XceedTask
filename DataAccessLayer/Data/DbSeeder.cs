using DataAccessLayer.Data.Seeders;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace DataAccessLayer.Data
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Seed Roles
                await SeedRoles(roleManager);

                // Seed Default Admin User
                await SeedAdminUser(userManager);

                // Seed Categories
                await CategorySeeder.SeedCategories(context);
            }
        }

        private static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            // Seed Roles
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }
            if (!await roleManager.RoleExistsAsync("User"))
            {
                await roleManager.CreateAsync(new IdentityRole("User"));
            }
        }

        private static async Task SeedAdminUser(UserManager<ApplicationUser> userManager)
        {
            // Seed Default Admin User
            var defaultUser = new ApplicationUser
            {
                UserName = "admin",
                Email = "admin@example.com",
                FirstName = "Admin",
                LastName = "Admin",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            if (userManager.Users.All(u => u.Id != defaultUser.Id))
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "Admin#123");
                    await userManager.AddToRoleAsync(defaultUser, "Admin");
                }
            }
        }
    }
}