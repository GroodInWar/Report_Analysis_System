using System.Text.Json.Serialization;

namespace server.DTOs;

public class LoginRequest
{
    [JsonPropertyName("emailOrUsername")]
    public string EmailOrUsername { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email
    {
        get => EmailOrUsername;
        set => EmailOrUsername = value;
    }

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;
}
