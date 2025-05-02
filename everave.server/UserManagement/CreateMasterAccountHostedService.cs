using AspNetCore.Identity.Mongo.Model;
using Microsoft.AspNetCore.Identity;

namespace everave.server.UserManagement;

public class CreateMasterAccountHostedService(IServiceProvider serviceProvider, IConfiguration configuration, ILogger<CreateMasterAccountHostedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating master account...");
        using var scope = serviceProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<MongoRole>>();

        if (!await roleManager.RoleExistsAsync(ApplicationUser.AdminRole))
        {
            await roleManager.CreateAsync(new MongoRole(ApplicationUser.AdminRole));
        }

        const string masterAccountUsername = "MasterAccount:Username";
        var username = configuration[masterAccountUsername];
        const string masterAccountPassword = "MasterAccount:Password";
        var password = configuration[masterAccountPassword];

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            logger.LogInformation($"{masterAccountUsername} or {masterAccountPassword} is not set");
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
                logger.LogInformation($"Master account created with username {username}");
            }
        }
        else
        {
            logger.LogInformation("Master account already exists");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}