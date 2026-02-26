using System.Text.Json.Serialization;

namespace WebApp_UnderTheHood.Authorization;

public class JsonWebToken
{
    [JsonPropertyName("token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("expiresAt")]
    public DateTime ExpiresAt { get; set; }
}
