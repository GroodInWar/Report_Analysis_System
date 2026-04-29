using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Models;

[Table("categories")]
public class Category
{
  [Key]
  [Column("category_id")]
  public uint category_id { get; set; }

  [Required]
  [Column("category_name")]
  [StringLength(50)]
  public string category_name { get; set; } = "";
}
