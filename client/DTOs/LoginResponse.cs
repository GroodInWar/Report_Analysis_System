namespace client.DTOs;

public class LoginResponse
{
  public string token { get; set; } = string.Empty;

  public CurrentUserResponse? user { get; set; }
}
