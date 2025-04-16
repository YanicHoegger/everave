using AspNetCore.Identity.Mongo.Model;
using Microsoft.AspNetCore.Identity;

namespace everave.server.UserManagement;

public class CreateMasterAccountHostedService(IServiceProvider serviceProvider, IConfiguration configuration) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<MongoRole>>();

        if (!await roleManager.RoleExistsAsync(ApplicationUser.AdminRole))
        {
            await roleManager.CreateAsync(new MongoRole(ApplicationUser.AdminRole));
        }

        var username = configuration["MasterAccount:Username"];
        var password = configuration["MasterAccount:Password"];

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            return;
        }

        var masterAccount = await userManager.FindByNameAsync(username);
        if (masterAccount == null)
        {
            masterAccount = new ApplicationUser
            {
                UserName = username,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(masterAccount, password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(masterAccount, ApplicationUser.AdminRole);
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}