using everave.server.UserManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver.Linq;

namespace everave.server.Components.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MentionsController(UserManager<ApplicationUser> userManager) : ControllerBase
    {
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetMentions([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Ok(new List<object>());

            query = query.ToLower();

            var users = await userManager.Users
                .Where(u => u.UserName.ToLower().Contains(query))
                .OrderBy(u => !u.UserName.ToLower().StartsWith(query)) // true = 1, false = 0 — puts prefix matches first
                .ThenBy(u => u.UserName.ToLower()) // 
                .Select(u => new
                {
                    id = u.Id.ToString(),
                    name = u.UserName
                })
                .Take(10)
                .ToListAsync();

            return Ok(users);
        }
    }
}
