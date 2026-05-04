using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Shared.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ReportStatus
{
  submitted,
  under_review,
  linked,
  closed,
  rejected
}

[Table("reports")]
public class Report
{
  [Key]
  [Column("report_id")]
  public uint report_id { get; set; }

  [Column("submitted_by_user_id")]
  public uint submitted_by_user_id { get; set; }

  [ForeignKey(nameof(submitted_by_user_id))]
  public User? SubmittedByUser { get; set; }

  [Column("incident_id")]
  public uint? incident_id { get; set; } = null;

  [ForeignKey(nameof(incident_id))]
  public Incident? Incident { get; set; }

  [Required]
  [Column("title")]
  [StringLength(150)]
  public string title { get; set; } = "";

  [Required]
  [Column("report_text")]
  public string report_text { get; set; } = "";

  [Column(
        "status",
        TypeName = "enum('submitted','under_review','linked','closed','rejected')"
  )]
  public ReportStatus status { get; set; } = ReportStatus.submitted;
    

  [Column("submitted_at")]
  public DateTime submitted_at { get; set; } = DateTime.UtcNow;

  [Column("updated_at")]
  public DateTime? updated_at { get; set; } = DateTime.UtcNow;
}
