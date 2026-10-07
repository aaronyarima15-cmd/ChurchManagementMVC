using ChurchManagementMVC.Models;
using Microsoft.AspNetCore.Identity;

namespace ChurchManagementMVC.Data
{
    public static class IdentitySeeder
    {
        public static async Task SeedRolesAsync(
            RoleManager<IdentityRole> roleManager)
        {
            string[] roles =
            {
                "Admin",
                "Pastor",
                "Secretary",
                "Treasurer",
                "Member"
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(
                        new IdentityRole(role));
                }
            }
        }
    }
}