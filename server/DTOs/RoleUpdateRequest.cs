using System.ComponentModel.DataAnnotations;

namespace server.DTOs;

public class RoleUpdateRequest
{
  [Required]
  [StringLength(50)]
  public string role_name { get; set; } = string.Empty;
}
