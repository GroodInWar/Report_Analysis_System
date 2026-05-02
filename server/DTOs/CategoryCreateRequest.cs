using System.ComponentModel.DataAnnotations;

namespace server.DTOs;

public class CategoryCreateRequest
{
    [Required]
    [StringLength(50)]
    public string category_name { get; set; } = string.Empty;
}
