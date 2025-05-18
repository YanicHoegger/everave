using System.Security.Claims;
using everave.server.Components.Pages;
using Microsoft.AspNetCore.Components.Authorization;
using MongoDB.Bson;

namespace everave.server.UserManagement
{
    public static class AuthenticationStateProviderExtensions
    {
        public static async Task<ObjectId?> GetUserId(this AuthenticationStateProvider authenticationStateProvider)
        {
            var authState = await authenticationStateProvider.GetAuthenticationStateAsync().ConfigureAwait(false);
            var user = authState.User;

            if (user.Identity?.IsAuthenticated ?? false)
            {
                return ObjectId.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            }

            return null;
        }

        public static async Task<bool> IsAdmin(this AuthenticationStateProvider authenticationStateProvider)
        {
            var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            return user.Identity?.IsAuthenticated == true && user.IsInRole(ApplicationUser.AdminRole);
        }
    }
}
