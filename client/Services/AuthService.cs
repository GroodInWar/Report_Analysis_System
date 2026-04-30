using client.DTOs;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace client.Services;

public class AuthService : AuthenticationStateProvider
{
  private readonly IHttpClientFactory _httpClientFactory;
  private string? _token;
  private CurrentUserResponse? _currentUser;

  public AuthService(IHttpClientFactory httpClientFactory)
  {
    _httpClientFactory = httpClientFactory;
  }

  private HttpClient Api => _httpClientFactory.CreateClient("Api");

  public override Task<AuthenticationState> GetAuthenticationStateAsync()
  {
    if (_currentUser == null || string.IsNullOrEmpty(_token))
    {
      return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
    }

    var claims = new List<Claim>
    {
      new(ClaimTypes.NameIdentifier, _currentUser.user_id.ToString()),
      new(ClaimTypes.Name, _currentUser.username),
      new(ClaimTypes.Email, _currentUser.email)
    };

    if (!string.IsNullOrWhiteSpace(_currentUser.role))
    {
      claims.Add(new Claim(ClaimTypes.Role, _currentUser.role));
    }

    var identity = new ClaimsIdentity(claims, "Bearer");
    return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
  }

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
    _currentUser = login?.user;

    var isLoggedIn = !string.IsNullOrEmpty(_token) && _currentUser != null;
    NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    return isLoggedIn;
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

  public HttpClient CreateAuthorizedApiClient()
  {
    var api = Api;
    AddBearerToken(api);
    return api;
  }

  public async Task Logout()
  {
    _token = null;
    _currentUser = null;
    NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
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
