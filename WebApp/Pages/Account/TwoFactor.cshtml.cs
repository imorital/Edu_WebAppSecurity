using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using WebApp.Data.Accounts;
using WebApp.Services;

namespace WebApp.Pages.Account;

public class TwoFactorModel : PageModel
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IEmailService _emailService;

    [BindProperty]
    public EmailMultiFactorAuth EmailMFA { get; set; } = new();

    public TwoFactorModel(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IEmailService emailService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailService = emailService;
    }

    public async Task OnGet(string email, bool rememberMe)
    {
        // Get the user
        var user = await _userManager.FindByEmailAsync(email);

        EmailMFA.SecurityCode = string.Empty;
        EmailMFA.RememberMe = rememberMe;

        if (user == null) {
            ModelState.AddModelError("Login2FA", "Invalid email.");
        }

        // Generate the security code
        var securityCode = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");

        // Send the security code to the user via email
        await _emailService.SendEmailAsync(email, 
            "Your Two-Factor Authentication Code", $"Your security code is: {securityCode}");
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _signInManager.TwoFactorSignInAsync(
            "Email",
            EmailMFA.SecurityCode,
            EmailMFA.RememberMe,
            rememberClient: false);

        if (result.Succeeded)
        {
            return RedirectToPage("/Index");
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError("Login2FA", "You are locked out.");
        }
        else
        {
            ModelState.AddModelError("Login2FA", "Failed to login.");
        }

        return Page();
    }
}

public class EmailMultiFactorAuth
{
    [Required]
    [DisplayName("Security Code")]
    public string SecurityCode { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}