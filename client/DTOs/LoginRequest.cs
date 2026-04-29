using System.ComponentModel.DataAnnotations;

namespace client.DTOs;

public class LoginRequest
{
  [Required(ErrorMessage = "Email or username is required.")]
  public string emailOrUsername { get; set; } = string.Empty;

  [Required(ErrorMessage = "Password is required.")]
  public string password { get; set; } = string.Empty;
}
