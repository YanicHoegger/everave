using everave.server.Components.Layout;
using everave.server.UserManagement;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace everave.server.Components.Controllers;

[Route("auth")]
public class AuthController(SignInManager<ApplicationUser> signInManager) : Controller
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromForm] Login.LoginModel model)
    {
        var result = await signInManager.PasswordSignInAsync(
            model.UserName, model.Password, model.IsPersistent, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            return Redirect("/forums");
        }

        // Handle login failure
        return Redirect("/login?error=true");
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return Redirect("/");
    }
}