using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApp.Data.Accounts;

namespace WebApp.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;

    public AccountController(SignInManager<User> signInManager, UserManager<User> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpGet]
    [Route("Account/ExternalLoginCallback")]
    public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
    {
        if (remoteError != null)
        {
            TempData["ErrorMessage"] = $"Error from external provider: {remoteError}";
            return RedirectToPage("/Account/Login");
        }

        var loginInfo = await _signInManager.GetExternalLoginInfoAsync();
        if (loginInfo == null)
        {
            TempData["ErrorMessage"] = "Error loading external login information.";
            return RedirectToPage("/Account/Login");
        }

        // Try to sign in with the external login provider
        var result = await _signInManager.ExternalLoginSignInAsync(
            loginInfo.LoginProvider, 
            loginInfo.ProviderKey, 
            isPersistent: false, 
            bypassTwoFactor: true);

        if (result.Succeeded)
        {
            return RedirectToPage("/Index");
        }

        if (result.IsLockedOut)
        {
            TempData["ErrorMessage"] = "Account is locked out.";
            return RedirectToPage("/Account/Login");
        }

        // If the user doesn't have an account, create one
        var emailClaim = loginInfo.Principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);

        if (emailClaim == null)
        {
            TempData["ErrorMessage"] = "Email claim not received from provider.";
            return RedirectToPage("/Account/Login");
        }

        var user = await _userManager.FindByEmailAsync(emailClaim.Value);

        if (user == null)
        {
            // Create new user
            user = new User
            {
                Email = emailClaim.Value,
                UserName = emailClaim.Value,
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                TempData["ErrorMessage"] = "Error creating user account.";
                return RedirectToPage("/Account/Login");
            }
        }

        // Add the external login to the user
        var addLoginResult = await _userManager.AddLoginAsync(user, loginInfo);
        if (!addLoginResult.Succeeded)
        {
            TempData["ErrorMessage"] = "Error adding external login.";
            return RedirectToPage("/Account/Login");
        }

        // Sign in the user
        await _signInManager.SignInAsync(user, isPersistent: false);

        return RedirectToPage("/Index");
    }
}
