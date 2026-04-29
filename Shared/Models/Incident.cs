using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Models;

[Table("incidents")]
public class Incident
{
  [Key]
  [Column("incident_id")]
  public uint incident_id { get; set; }

  [Required]
  [Column("created_by_user_id")]
  public uint CreatedByUserId { get; set; }

  [ForeignKey(nameof(CreatedByUserId))]
  public User? CreatedByUser { get; set; }

  [Required]
  [Column("category_id")]
  public uint category_id { get; set; }

  [Required]
  [Column("severity_id")]
  public uint severity_id { get; set; }

  [Required]
  [Column("title")]
  [StringLength(150)]
  public string incident_title { get; set; } = "";

  [Required]
  [Column("description")]
  public string incident_description { get; set; } = "";

  [Column("created_at")]
  public DateTime created_at { get; set; } = DateTime.UtcNow;

  [Required]
  [Column("updated_at")]
  public DateTime updated_at { get; set; }

  [Column("resolved_at")]
  public DateTime? resolved_at { get; set; } = null;

  public ICollection<Report> Reports { get; set; } = new List<Report>();
}