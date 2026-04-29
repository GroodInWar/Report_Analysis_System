using System.ComponentModel.DataAnnotations;

namespace client.DTOs;

public class RegisterRequest
{
  [Required(ErrorMessage = "First name is required.")]
  public string first_name { get; set; } = string.Empty;

  [Required(ErrorMessage = "Last name is required.")]
  public string last_name { get; set; } = string.Empty;

  [Required(ErrorMessage = "Username is required.")]
  public string username { get; set; } = string.Empty;

  [Required(ErrorMessage = "Email is required.")]
  [EmailAddress(ErrorMessage = "Enter a valid email address.")]
  [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Email must be in a valid format.")]
  public string email { get; set; } = string.Empty;

  [Required(ErrorMessage = "Password is required.")]
  [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
  [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d).+$", ErrorMessage = "Password must include uppercase, lowercase, and number characters.")]
  public string password { get; set; } = string.Empty;
}
