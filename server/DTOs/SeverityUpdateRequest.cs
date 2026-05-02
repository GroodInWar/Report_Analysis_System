using System.ComponentModel.DataAnnotations;

namespace server.DTOs;

public class SeverityUpdateRequest
{
  [Required]
  [StringLength(50)]
  public string severity_name { get; set; } = string.Empty;
}
