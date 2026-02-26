using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using WebAPI.Models;

namespace WebAPI.Controllers;

[Route("[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost]
    public IActionResult Authenticate([FromBody] Credential credential)
    {
        // For demonstration purposes, we will just check if the username and password are "admin" and password
        if (credential.UserName == "admin" && credential.Password == "password")
        {
            // Create the security context and set the user as authenticated
            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, credential.UserName),
                    new Claim(ClaimTypes.Email, $"{credential.UserName}@somedomain.com"),
                    new Claim("Department", "HR"),  // Custom claim for authorisation policy.
                    new Claim("Admin", "true"),  // Custom claim for admin. Value can be anything, but good to have it make sense.
                    new Claim("Manager", "true"),  // Custom claim for manager. Value can be anything, but good to have it make sense.
                    new Claim("EmploymentDate", DateTime.UtcNow.AddYears(-1).ToString("yyyy-MM-dd"))
                };

            var expiresAt = DateTime.UtcNow.AddMinutes(10);

            return Ok(new
            {
                Token = GenerateToken(claims, expiresAt),
                ExpiresAt = expiresAt
            });
        }

        ModelState.AddModelError("Unauthorized", "Invalid username or password.");
        var problemDetails = new ValidationProblemDetails(ModelState)
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Unauthorized",
            Detail = "Invalid username or password."
        };

        return Unauthorized(problemDetails);
    }

    private string GenerateToken(List<Claim> claims, DateTime expiresAt)
    {
        var secretKey = _configuration["Jwt:Secret"] ?? string.Empty;

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)), 
                SecurityAlgorithms.HmacSha256Signature),
            NotBefore = DateTime.UtcNow
        };

        var tokenHandler = new JsonWebTokenHandler();
        return tokenHandler.CreateToken(tokenDescriptor);
    }
}
