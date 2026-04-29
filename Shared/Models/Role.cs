using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Shared.Models;

[Table("roles")]
[Index(nameof(role_name), IsUnique = true)]
public class Role
{
  [Key]
  [Column("role_id")]
  public uint role_id { get; set; }

  [Required]
  [Column("role_name")]
  [StringLength(50)]
  public string role_name { get; set; } = "";
}
