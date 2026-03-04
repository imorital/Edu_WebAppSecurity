using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using System.Reflection.Metadata.Ecma335;
using WebApp.Services;
using WebApp.Data.Accounts;

namespace WebApp.Pages;

public class RegisterModel : PageModel
{
    private readonly UserManager<User> _userManager;
    private readonly IEmailService _emailService;
    private readonly IWebHostEnvironment _environment;

    [BindProperty]
    public RegisterViewModel RegisterViewModel { get; set; } = new RegisterViewModel();

    public RegisterModel(UserManager<User> userManager, IEmailService emailService, IWebHostEnvironment environment)
    {
        _userManager = userManager;
        _emailService = emailService;
        _environment = environment;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        // Validate the Email address does not exist (optional as we added a check)

        // Create the user
        var user = new User
        {
            UserName = RegisterViewModel.Email,
            Email = RegisterViewModel.Email,
            Department = RegisterViewModel.Department,
            Position = RegisterViewModel.Position
        };

        var result = await _userManager.CreateAsync(user, RegisterViewModel.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return Page();
        }

        // Successful registration
        //Generate a token and send email confirmation
        var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        var confirmationLink = Url.PageLink(
            pageName: "/Account/ConfirmEmail",
            values: new { userId = user.Id, token = confirmationToken }
            );

        if (_environment.IsDevelopment())
        {
            // Development environment - skip email sending and just confirm directly
            return Redirect(confirmationLink ?? "");
        }

        await _emailService.SendEmailAsync(
            user.Email, 
            "Confirm your email", 
            $"Please confirm your account by clicking this link: <a href='{confirmationLink}'>Confirm Email</a>");

        return RedirectToPage("/Account/Login");
    }
}

public class RegisterViewModel
{
    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 8)]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required]
    public String Department { get; set; } = string.Empty;

    [Required]
    public String Position { get; set; } = string.Empty;
}
