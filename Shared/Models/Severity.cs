using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Shared.Models;

[Table("severities")]
[Index(nameof(severity_name), IsUnique = true)]
public class Severity
{
  [Key]
  [Column("severity_id")]
  public uint severity_id { get; set; }

  [Required]
  [Column("severity_name")]
  [StringLength(50)]
  public string severity_name { get; set; } = "";
}
