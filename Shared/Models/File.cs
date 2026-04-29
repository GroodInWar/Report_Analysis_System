using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Models;

[Table("files")]
public class File
{
  [Key]
  [Column("file_id")]
  public uint file_id { get; set; }

  [Required]
  [Column("file_name")]
  [StringLength(255)]
  public string file_name { get; set; } = "";

  [Required]
  [Column("file_path")]
  [StringLength(500)]
  public string file_path { get; set; } = "";

  [Required]
  [Column("file_hash")]
  [StringLength(64)]
  public string file_hash { get; set; } = "";

  [Required]
  [Column("uploaded_at")]
  public DateTime uploaded_at { get; set; } = DateTime.UtcNow;
}
