using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using WebApp.Data.Accounts;

namespace WebApp.Pages.Account;

public class UserProfileModel : PageModel
{
    private readonly UserManager<User> _userManager;

    [BindProperty]
    public UserProfileViewModel UserProfile { get; set; } = new UserProfileViewModel();

    [BindProperty]
    public string? SuccessMessage { get; set; }

    public UserProfileModel(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        SuccessMessage = string.Empty;

        var (user, managerClaim, isAdmin) = await GetUserInfoAsync();

        if (user == null)
        {
            return NotFound();
        }

        UserProfile.Email = User.Identity?.Name ?? string.Empty;
        UserProfile.Department = user.Department;
        UserProfile.Position = user.Position;
        UserProfile.Manager = managerClaim?.Value ?? "Unknown";
        UserProfile.IsAdmin = isAdmin;

        SuccessMessage = "Profile updated successfully.";
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Handle profile updates if needed
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var (user, managerClaim, sAdmin) = await GetUserInfoAsync();

            if (user == null)
            {
                return NotFound();
            }

            if (managerClaim != null)
                await _userManager.ReplaceClaimAsync(user, managerClaim, new Claim(managerClaim.Type, UserProfile.Manager));

            var roles = await _userManager.GetRolesAsync(user);
            if (roles != null)
            {
                if (UserProfile.IsAdmin && !roles.Contains("Admin"))
                {
                    await _userManager.AddToRoleAsync(user, "Admin");
                }
                else if (!UserProfile.IsAdmin && roles.Contains("Admin"))
                {
                    await _userManager.RemoveFromRoleAsync(user, "Admin");
                }
            }

            if (user.Department != UserProfile.Department)
                user.Department = UserProfile.Department;
            if (user.Position != UserProfile.Position)
                user.Position = UserProfile.Position;

            await _userManager.UpdateAsync(user);
        }
        catch (Exception)
        {
            ModelState.AddModelError("UserProfile", "An error occurred while updating your profile. Please try again.");
        }

        return Page();
    }

    private async Task<(User? user, Claim? managerClaim, bool isAdmin)> GetUserInfoAsync()
    {
        var user = await _userManager.FindByNameAsync(User.Identity?.Name ?? "");

        if (user == null)
        {
            return (null, null, false);
        }

        var claims = await _userManager.GetClaimsAsync(user);
        // Department & Position are properties on the User class
        var managerClaim = claims.FirstOrDefault(c => c.Type == "Manager");

        var roles = await _userManager.GetRolesAsync(user);
        var isAdmin = roles.Contains("Admin");

        return (user, managerClaim, isAdmin);
    }
}

public class UserProfileViewModel
{
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Department { get; set; } = string.Empty;

    [Required]
    public string Position { get; set; } = string.Empty;

    [Required]
    public string Manager { get; set; } = string.Empty;

    public bool IsAdmin { get; set; }
}
