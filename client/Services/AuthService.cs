using client.DTOs;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace client.Services;

public class AuthService : AuthenticationStateProvider
{
  private readonly IHttpClientFactory _httpClientFactory;
  private readonly ProtectedLocalStorage _localStorage;
  private string? _token;
  private CurrentUserResponse? _currentUser;
  private bool _hasTriedRestore;
  private const string AuthStorageKey = "file-analysis-auth";

  public AuthService(IHttpClientFactory httpClientFactory, ProtectedLocalStorage localStorage)
  {
    _httpClientFactory = httpClientFactory;
    _localStorage = localStorage;
  }

  private HttpClient Api => _httpClientFactory.CreateClient("Api");

  public override async Task<AuthenticationState> GetAuthenticationStateAsync()
  {
    await RestoreSessionFromStorage();

    if (_currentUser == null || string.IsNullOrEmpty(_token))
    {
      return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
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
    return new AuthenticationState(new ClaimsPrincipal(identity));
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
    if (isLoggedIn)
    {
      await _localStorage.SetAsync(AuthStorageKey, new AuthSession
      {
        token = _token!,
        user = _currentUser!
      });
    }

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
    await _localStorage.DeleteAsync(AuthStorageKey);
    NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
  }

  private void AddBearerToken(HttpClient api)
  {
    if (!string.IsNullOrEmpty(_token))
    {
      api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
    }
  }

  private async Task RestoreSessionFromStorage()
  {
    if (_hasTriedRestore || !string.IsNullOrEmpty(_token))
    {
      return;
    }

    _hasTriedRestore = true;

    try
    {
      var storedSession = await _localStorage.GetAsync<AuthSession>(AuthStorageKey);
      if (storedSession.Success &&
        storedSession.Value != null &&
        !string.IsNullOrWhiteSpace(storedSession.Value.token))
      {
        _token = storedSession.Value.token;
        _currentUser = storedSession.Value.user;
      }
    }
    catch (InvalidOperationException)
    {
      _hasTriedRestore = false;
    }
  }

  private sealed class AuthSession
  {
    public string token { get; set; } = "";
    public CurrentUserResponse? user { get; set; }
  }
}
