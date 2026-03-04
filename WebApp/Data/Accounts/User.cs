using Microsoft.AspNetCore.Identity;

namespace WebApp.Data.Accounts;

public class User : IdentityUser
{
    public String Department { get; set; } = string.Empty;
    public String Position { get; set; } = string.Empty;
}
