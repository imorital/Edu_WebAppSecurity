using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QRCoder;
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

        GenerateQrCode(user.Email, key);
        
        SetupMFA.Key = key;
    }

    private void GenerateQrCode(string? userEmail, string? secretKey)
    {
        var appName = "Udemy Security Web App";

        // 1. Format the URI
        // Format: otpauth://totp/{Issuer}:{User}?secret={Key}&issuer={Issuer}
        string uri = $"otpauth://totp/{Uri.EscapeDataString(appName)}:{Uri.EscapeDataString(userEmail)}?secret={secretKey}&issuer={Uri.EscapeDataString(appName)}";

        // 2. Generate the QR Code
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(uri, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);

        // 3. Convert to Base64 to pass to the view
        byte[] qrCodeImage = qrCode.GetGraphic(20);
        SetupMFA.QrCodeBase64 = Convert.ToBase64String(qrCodeImage);
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

    public string? QrCodeBase64 { get; internal set; }
}