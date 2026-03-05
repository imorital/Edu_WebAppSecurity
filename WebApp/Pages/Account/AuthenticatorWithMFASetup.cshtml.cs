using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using WebApp.Data.Accounts;

namespace WebApp.Pages.Account;

[Authorize]
public class AuthenticatorWithMFASetupModel : PageModel
{
    private readonly UserManager<User> _userManager;

    [BindProperty]
    public SetupMFAViewModel SetupMFA { get; set; }

    [BindProperty]
    public bool Succeeded { get; set; }

    public AuthenticatorWithMFASetupModel(UserManager<User> userManager)
    {
        _userManager = userManager;
        SetupMFA = new SetupMFAViewModel();
        Succeeded = false;
    }

    public async Task OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null) {
            ModelState.AddModelError("User", "User not found.");
            return;
        }

        await _userManager.ResetAuthenticatorKeyAsync(user);
        var key = await _userManager.GetAuthenticatorKeyAsync(user);
        
        SetupMFA.Key = key;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null) {
            ModelState.AddModelError("User", "User not found.");
            return Page();
        }

        var isValid = await _userManager.VerifyTwoFactorTokenAsync(
            user,
            _userManager.Options.Tokens.AuthenticatorTokenProvider,
            SetupMFA.SecurityCode);

        if (!isValid)
        {
            ModelState.AddModelError("SecurityCode", "Invalid security code.");
            return Page();
        }

        await _userManager.SetTwoFactorEnabledAsync(user, true);
        Succeeded = true;
        // Optionally, you can add a success message or redirect to another page
        return Page();
    }
}

public class SetupMFAViewModel
{
    public string? Key { get; set; }

    [Required]
    [DisplayName("Security Code")]
    public string SecurityCode { get; set; } = string.Empty;
}