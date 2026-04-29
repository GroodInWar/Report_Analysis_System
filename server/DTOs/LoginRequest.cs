namespace server.DTOs;

public class LoginRequest
{
    public string EmailOrUsername { get; set; } = string.Empty;

    public string Email
    {
        get => EmailOrUsername;
        set => EmailOrUsername = value;
    }

    public string Password { get; set; } = string.Empty;
}
