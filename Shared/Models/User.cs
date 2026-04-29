using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Models;

[Table("users")]
public class User
{
  [Key]
  [Column("user_id")]
  public uint user_id { get; set; }

  [Column("role_id")]
  [Required]
  public uint role_id { get; set; }

  [Column("first_name")]
  [Required]
  public string first_name { get; set; } = "";

  [Column("last_name")]
  [Required]
  public string last_name { get; set; } = "";

  [Column("username")]
  [Required]
  [StringLength(50)]
  public string username { get; set; } = "";

  [Column("email")]
  [Required]
  [StringLength(255)]
  public string email { get; set; } = "";

  [Column("password_hash")]
  [Required]
  public string password_hash { get; set; } = "";

  [Column("created_at")]
  public DateTime created_at { get; set; } = DateTime.UtcNow;

  [Column("updated_at")]
  public DateTime? updated_at { get; set; } = DateTime.UtcNow;

  [Column("last_login_at")]
  public DateTime? last_login_at { get; set; } = null;

  // All users can create reports
  public ICollection<Report> CreatedReports { get; set; } = new List<Report>();

  // Only analysts can be assigned to incidents
  public ICollection<Incident> CreatedIncidents { get; set; } = new List<Incident>();
}
