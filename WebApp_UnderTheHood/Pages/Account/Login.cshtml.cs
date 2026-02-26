using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using System.Security.Claims;
using WebApp_UnderTheHood.Authorization;

namespace WebApp_UnderTheHood.Pages.Account
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public Credential Credential { get; set; } = new Credential();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();


            // For demonstration purposes, we will just check if the username and password are "admin" and password
            if (Credential.UserName == "admin" && Credential.Password == "password")
            {
                // Create the security context and set the user as authenticated
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, Credential.UserName),
                    new Claim(ClaimTypes.Email, $"{Credential.UserName}@somedomain.com"),
                    new Claim("Department", "HR"),  // Custom claim for authorisation policy.
                    new Claim("Admin", "true"),  // Custom claim for admin. Value can be anything, but good to have it make sense.
                    new Claim("Manager", "true"),  // Custom claim for manager. Value can be anything, but good to have it make sense.
                    new Claim("EmploymentDate", DateTime.UtcNow.AddYears(-1).ToString("yyyy-MM-dd"))
                };

                var identity = new ClaimsIdentity(claims, "MyCookieAuth");
                ClaimsPrincipal principal = new ClaimsPrincipal(identity);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = Credential.RememberMe,
                    ExpiresUtc = Credential.RememberMe ? DateTimeOffset.UtcNow.AddMinutes(2) : null
                };

                // Encrypt the principal
                await HttpContext.SignInAsync("MyCookieAuth", principal, authProperties);

                // In a real application, you would set up authentication and redirect to a secure page
                return RedirectToPage("/Index");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }
        }
    }
}
