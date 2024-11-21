using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using MultiplexCinema.Models;
using System;
using System.Threading.Tasks;

public class DataSeeder
{
    public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        
        if (await roleManager.FindByNameAsync("Admin") == null)
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        if (await roleManager.FindByNameAsync("User") == null)
        {
            await roleManager.CreateAsync(new IdentityRole("User"));
        }
        
        var adminUser = await userManager.FindByEmailAsync("admin@kino.com");
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = "admin@kino.com",
                Email = "admin@kino.com"
            };
            await userManager.CreateAsync(adminUser, "AdminPassword123!");
        }
        
        var normalUser = await userManager.FindByEmailAsync("user@kino.com");
        if (normalUser == null)
        {
            normalUser = new ApplicationUser
            {
                UserName = "user@kino.com",
                Email = "user@kino.com"
            };
            await userManager.CreateAsync(normalUser, "UserPassword123!");
        }
        
        if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }

        if (!await userManager.IsInRoleAsync(normalUser, "User"))
        {
            await userManager.AddToRoleAsync(normalUser, "User");
        }
    }
}
