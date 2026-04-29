using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Shared.Models;

[Table("report_files")]
[PrimaryKey(nameof(report_id), nameof(file_id))]
public class Report_File
{  
  [Required]
  [Column("report_id")]
  public uint report_id { get; set; }

  [ForeignKey(nameof(report_id))]
  public Report? Report { get; set; }

  [Required]
  [Column("file_id")]
  public uint file_id { get; set; }

  [ForeignKey(nameof(file_id))]
  public File? File { get; set; }
}
