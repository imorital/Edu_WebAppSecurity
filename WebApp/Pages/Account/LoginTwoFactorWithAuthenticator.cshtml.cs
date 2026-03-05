using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using WebApp.Data.Accounts;

namespace WebApp.Pages.Account;

public class LoginTwoFactorWithAuthenticatorModel : PageModel
{
    private readonly SignInManager<User> _signInManager;

    [BindProperty]
    public AuthenticatorMFAViewModel AuthenticatorMFA { get; set; }

    public LoginTwoFactorWithAuthenticatorModel(SignInManager<User> signInManager)
    {
        AuthenticatorMFA = new AuthenticatorMFAViewModel();
        _signInManager = signInManager;
    }

    public void OnGet(bool rememberMe)
    {
        AuthenticatorMFA.SecurityCode = string.Empty;
        AuthenticatorMFA.RememberMe = rememberMe;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(
            AuthenticatorMFA.SecurityCode,
            AuthenticatorMFA.RememberMe,
            false);

        if (result.Succeeded)
            return RedirectToPage("/Index");

        if (result.IsLockedOut)
            ModelState.AddModelError("Login2FA", "You are locked out.");
        else
            ModelState.AddModelError("Login2FA", "Failed to login.");

        return Page();
    }

    public class AuthenticatorMFAViewModel
    {
        [Required, DisplayName("Code")]
        public string SecurityCode { get; set; } = string.Empty;

        [BindProperty]
        public bool RememberMe { get; set; }
    }
}