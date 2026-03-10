using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using WebApp.Data.Accounts;

namespace WebApp.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;

        [BindProperty]
        public CredentialViewModel Credential { get; set; } = new CredentialViewModel();

        [BindProperty]
        public IEnumerable<AuthenticationScheme> ExternalLoginProviders { get; set; } = Enumerable.Empty<AuthenticationScheme>();

        public LoginModel(SignInManager<User> signInManager)
        {
            _signInManager = signInManager;
        }

        public async Task OnGetAsync()
        {
            // Get list of external providers (authentication schemes)
            ExternalLoginProviders = await _signInManager.GetExternalAuthenticationSchemesAsync();


        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var result = await _signInManager.PasswordSignInAsync(
                Credential.Email,
                Credential.Password,
                Credential.RememberMe,
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return RedirectToPage("/Index");
            }

            if (result.RequiresTwoFactor)
            {
                return RedirectToPage("/Account/LoginTwoFactorWithAuthenticator",
                    new { 
                        Credential.RememberMe
                    });
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError("Login", "You are locked out.");
            }
            else
            {
                ModelState.AddModelError("Login", "Failed to login.");
            }

            return Page();
        }

        public IActionResult OnPostLoginExternally(string provider)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account");
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }
    }

    public class CredentialViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }
    }
}
