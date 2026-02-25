using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp_UnderTheHood.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        public void OnGet()
        {

        }
    }
}
