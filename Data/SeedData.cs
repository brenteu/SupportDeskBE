using Microsoft.AspNetCore.Identity;

namespace SupportDeskBE.Data
{
    public static class SeedData
    {
        public static async Task EnsureSeededAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

            string[] roles = { "Customer", "Agent", "Admin" };
            foreach (var r in roles)
            {
                if (!await roleMgr.RoleExistsAsync(r))
                    await roleMgr.CreateAsync(new IdentityRole(r));
            }

            // Default admin (change password later)
            var adminEmail = "admin@supportdesk.local";
            var admin = await userMgr.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                admin = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
                await userMgr.CreateAsync(admin, "Admin123!"); // change after first run
                await userMgr.AddToRoleAsync(admin, "Admin");
            }

            // Default agent
            var agentEmail = "agent@supportdesk.local";
            var agent = await userMgr.FindByEmailAsync(agentEmail);
            if (agent == null)
            {
                agent = new IdentityUser { UserName = agentEmail, Email = agentEmail, EmailConfirmed = true };
                await userMgr.CreateAsync(agent, "Agent123!");
                await userMgr.AddToRoleAsync(agent, "Agent");
            }
        }
    }
}

