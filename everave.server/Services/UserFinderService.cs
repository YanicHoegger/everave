using everave.server.UserManagement;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver.Linq;

namespace everave.server.Services
{
    public class UserFinderService(UserManager<ApplicationUser> userManager)
    {
        public async Task<List<ApplicationUser>> FindUsers(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return [];

            query = query.ToLower();
            var users = await userManager.Users
                .Where(u => u.UserName.ToLower().Contains(query))
                .OrderBy(u => !u.UserName.ToLower().StartsWith(query)) 
                .ThenBy(u => u.UserName.ToLower()) 
                .Take(10)
                .ToListAsync();

            return users;
        }
    }
}
