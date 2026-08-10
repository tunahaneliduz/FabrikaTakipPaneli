using Microsoft.AspNetCore.Identity;
using FabrikaTakipPaneli.Authorization;

namespace FabrikaTakipPaneli.Data;

public static class RoleSeeder
{
    public static readonly string[] Roles = { AppRoles.Admin, AppRoles.Personel };

    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        foreach (var role in Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var configuration = services.GetRequiredService<IConfiguration>();
        var initialAdminEmail = configuration["SeedAdmin:Email"];

        if (!string.IsNullOrWhiteSpace(initialAdminEmail))
        {
            var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
            var adminUser = await userManager.FindByEmailAsync(initialAdminEmail);

            if (adminUser is not null && !await userManager.IsInRoleAsync(adminUser, AppRoles.Admin))
            {
                await userManager.AddToRoleAsync(adminUser, AppRoles.Admin);
            }
        }
    }
}
