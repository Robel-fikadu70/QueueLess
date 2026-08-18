using Microsoft.AspNetCore.Identity;
using QueueLess.Infrastructure.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QueueLess.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task SeedAsync(QlDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        // 1. Ensure the database matches our migration schema
        await context.Database.EnsureCreatedAsync();

        // 2. Seed Default Roles
        string[] roles = ["Admin", "Staff", "Customer"];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // 3. Seed Default Admin User
        var adminEmail = "admin@queueless.com";
        var existingAdmin = await userManager.FindByEmailAsync(adminEmail);

        if (existingAdmin == null)
        {
            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "System",
                LastName = "Administrator",
                EmailConfirmed = true
            };

            // Password meets the guidelines configured in Program.cs
            var result = await userManager.CreateAsync(adminUser, "Admin123!");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Failed to seed Admin user: {errors}");
            }
        }
    }
}