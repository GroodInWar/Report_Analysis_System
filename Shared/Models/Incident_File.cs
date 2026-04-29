using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Shared.Models;

[Table("incident_files")]
[PrimaryKey(nameof(incident_id), nameof(file_id))]
public class Incident_File
{  
  [Required]
  [Column("incident_id")]
  public uint incident_id { get; set; }

  [ForeignKey(nameof(incident_id))]
  public Incident? Incident { get; set; }

  [Required]
  [Column("file_id")]
  public uint file_id { get; set; }

  [ForeignKey(nameof(file_id))]
  public File? File { get; set; }
}
