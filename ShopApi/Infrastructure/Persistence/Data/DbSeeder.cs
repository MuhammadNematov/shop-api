using Microsoft.Extensions.DependencyInjection;
using Application.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(
        IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var roleManager = scope.ServiceProvider
            .GetRequiredService<RoleManager<IdentityRole<int>>>();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<User>>();

        await context.Database.MigrateAsync();

        string[] roles = { "Admin", "User" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(
                    new IdentityRole<int>(role));
            }
        }

        const string adminEmail = "admin@mail.com";
        const string adminPassword = "Admin123";

        var admin = await userManager.FindByEmailAsync(adminEmail);

        if (admin is null)
        {
            admin = new User
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Admin",
                LastName = "Administrator",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(
                admin, adminPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    result.Errors.Select(e => e.Description));

                throw new InvalidOperationException(
                    $"Could not create admin: {errors}");
            }
        }

        if (!await userManager.IsInRoleAsync(admin, "Admin"))
        {
            var result = await userManager.AddToRoleAsync(
                admin, "Admin");

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    "Could not assign the Admin role.");
            }
        }
    }
}