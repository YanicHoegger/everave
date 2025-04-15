using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;

namespace everave.server.UserManagement;

public class MongoAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<ApplicationUser> _userManager;

    public MongoAuthenticationStateProvider(
        IHttpContextAccessor httpContextAccessor,
        UserManager<ApplicationUser> userManager)
    {
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (user?.Identity?.IsAuthenticated == true)
        {
            var appUser = await _userManager.GetUserAsync(user);
            if (appUser != null)
            {
                var claims = new List<Claim>
                {
                    new(ClaimTypes.Name, appUser.UserName),
                    new(ClaimTypes.Email, appUser.Email ?? string.Empty)
                };

                var roles = await _userManager.GetRolesAsync(appUser);
                claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

                var identity = new ClaimsIdentity(claims, "MongoDbAuth");
                return new AuthenticationState(new ClaimsPrincipal(identity));
            }
        }

        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }

    public void NotifyAuthenticationStateChanged(Task<AuthenticationState> authenticationStateTask)
    {
        base.NotifyAuthenticationStateChanged(authenticationStateTask);
    }
}