using client.DTOs;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;

namespace client.Services;

public class AuthService 
{
  private readonly IHttpClientFactory _httpClientFactory;
  private string? _token;

  public AuthService(IHttpClientFactory httpClientFactory)
  {
    _httpClientFactory = httpClientFactory;
  }

  private HttpClient Api => _httpClientFactory.CreateClient("Api");

  public async Task<bool> Register(RegisterRequest request)
  {
    var hashedRequest = new RegisterRequest
    {
      first_name = request.first_name,
      last_name = request.last_name,
      username = request.username,
      email = request.email,
      password = HashPassword(request.password)
    };

    var response = await Api.PostAsJsonAsync("api/auth/register", hashedRequest);
    return response.IsSuccessStatusCode;
  }

  private static string HashPassword(string password)
  {
    var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
    return Convert.ToHexString(bytes).ToLowerInvariant();
  }

  public async Task<bool> Login(LoginRequest request)
  {
    var hashedRequest = new LoginRequest
    {
      emailOrUsername = request.emailOrUsername,
      password = HashPassword(request.password)
    };

    var response = await Api.PostAsJsonAsync("api/auth/login", hashedRequest);
    if (!response.IsSuccessStatusCode)
    {
      return false;
    }

    var login = await response.Content.ReadFromJsonAsync<LoginResponse>();
    _token = login?.token;
    return !string.IsNullOrEmpty(_token);
  }

  public async Task<CurrentUserResponse?> GetCurrentUser()
  {
    var api = Api;
    AddBearerToken(api);

    var response = await api.GetAsync("api/auth/current-user");
    if (!response.IsSuccessStatusCode)
    {
      return null;
    }
    return await response.Content.ReadFromJsonAsync<CurrentUserResponse>();
  }

  public async Task Logout()
  {
    _token = null;
    await Task.CompletedTask;
  }

  private void AddBearerToken(HttpClient api)
  {
    if (!string.IsNullOrEmpty(_token))
    {
      api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
    }
  }
}
