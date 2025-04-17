using System.Security.Claims;
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
    }
}
