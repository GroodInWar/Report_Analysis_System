using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Models;

[Table("comments")]
public class Comment
{
  [Key]
  [Column("comment_id")]
  public uint comment_id { get; set; }

  [Required]
  [Column("incident_id")]
  public uint incident_id { get; set; }

  [ForeignKey(nameof(incident_id))]
  public Incident? Incident { get; set; }

  [Required]
  [Column("user_id")]
  public uint user_id { get; set; }

  [ForeignKey(nameof(user_id))]
  public User? User { get; set; }

  [Required]
  [Column("comment_text")]
  public string comment_text { get; set; } = "";

  [Column("created_at")]
  public DateTime created_at { get; set; } = DateTime.UtcNow;

  [Column("updated_at")]
  public DateTime? updated_at { get; set; } = DateTime.UtcNow;
}
