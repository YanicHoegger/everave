using everave.server.UserManagement;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace everave.server.Components.Controllers;

[Route("auth")]
public class AuthController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager) : Controller
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromForm] LoginModel model, [FromQuery] string? returnUrl)
    {
        var result = await signInManager.PasswordSignInAsync(
            model.UserName, model.Password, model.IsPersistent, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            return Redirect(returnUrl ?? "/"); ;
        }

        return Ok("Falsches login");
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return Redirect("/");
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromForm] RegisterModel model)
    {
        if (model.Password != model.ConfirmPassword)
        {
            return BadRequest("Passwords do not match.");
        }

        var user = new ApplicationUser { UserName = model.UserName };
        var result = await userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            await signInManager.SignInAsync(user, isPersistent: false);
            return Redirect($"/user-details/{user.Id}");
        }

        return BadRequest($"Registration failed. {string.Join(", ", result.Errors)}");
    }

    public class LoginModel
    {
        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        public bool IsPersistent { get; set; } = false;
    }

    public class RegisterModel
    {
        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}