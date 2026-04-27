using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Models;

public enum ReportStatus
{
  submitted,
  under_review,
  linked,
  closed,
  rejected
}

public class Report
{
  [Key]
  public int report_id { get; set; }

  [ForeignKey("User")]
  public int submitted_by_user_id { get; set; }

  public int ? incident_id { get; set; } = null;

  [Required]
  public string title { get; set; } = "";

  [Required]
  public string report_text { get; set; } = "";

  public ReportStatus status { get; set; } = ReportStatus.submitted;
  public DateTime submitted_at { get; set; } = DateTime.UtcNow;
  public DateTime? updated_at { get; set; } = DateTime.UtcNow;
}