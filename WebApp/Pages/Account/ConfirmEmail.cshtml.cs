using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.Account;

public class ConfirmEmailModel : PageModel
{
    [BindProperty]
    public string Message { get; set; } = string.Empty;

    private readonly UserManager<IdentityUser> _userManager;

    public ConfirmEmailModel(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IActionResult> OnGetAsync(string userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user != null)
        {
            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                Message = "Email confirmed successfully. You may now login.";
                return Page();
            }
        }

        Message = "Email validation failed.";
        return Page();
    }
}
