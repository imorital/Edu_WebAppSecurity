using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;
using WebApp_UnderTheHood.Authorization;
using WebApp_UnderTheHood.DTOs;

namespace WebApp_UnderTheHood.Pages;

[Authorize(Policy = "HRManagerOnly")]
public class HrManagerModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    public List<WeatherForecastDTO>? WeatherForecastItems { get; private set; }

    public HrManagerModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task OnGetAsync()
    {
        JsonWebToken token = await GetOrCreateJWT();

        var client = _httpClientFactory.CreateClient("OurWebAPI");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", token?.AccessToken ?? string.Empty);

        WeatherForecastItems = await client.GetFromJsonAsync<List<WeatherForecastDTO>>("WeatherForecast");
    }

    private async Task<JsonWebToken> GetOrCreateJWT()
    {
        // Get JWT from the cookie and set it in the HttpClient's Authorization header to call the API.
        var token = new JsonWebToken();

        var strTokenObj = HttpContext.Session.GetString("JWT");
        if (!string.IsNullOrEmpty(strTokenObj))
        {
            token = JsonSerializer.Deserialize<JsonWebToken>(strTokenObj) ?? new JsonWebToken();

            if (token == null || token.ExpiresAt < DateTime.UtcNow)
            {
                token = await Authenticate();
            }
        }
        else
        {
            token = await Authenticate();
        }

        return token;
    }

    private async Task<JsonWebToken> Authenticate()
    {
        var client = _httpClientFactory.CreateClient("OurWebAPI");

        // Simulate login to get the cookie
        var response = await client.PostAsJsonAsync("auth",
            new { UserName = "admin", Password = "password" });
        response.EnsureSuccessStatusCode(); // Ensure the login was successful otherwise throw an exception
        string jwtString = await response.Content.ReadAsStringAsync();

        // Store in the session for future use
        HttpContext.Session.SetString("JWT", jwtString);

        var token = JsonSerializer.Deserialize<JsonWebToken>(jwtString) ?? new JsonWebToken();
        return token;
    }
}
