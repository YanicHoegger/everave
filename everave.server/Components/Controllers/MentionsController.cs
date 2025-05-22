using everave.server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace everave.server.Components.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MentionsController(UserFinderService userFinder) : ControllerBase
    {
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetMentions([FromQuery] string query)
        {
            var users = (await userFinder.FindUsers(query))
                .Select(u => new
                {
                    id = u.Id.ToString(),
                    name = u.UserName
                })
                .ToList();

            return Ok(users);
        }
    }
}
