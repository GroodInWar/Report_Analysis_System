using System.ComponentModel.DataAnnotations;

namespace server.DTOs;

public class CategoryUpdateRequest
{
    [Required]
    [StringLength(50)]
    public string category_name { get; set; } = string.Empty;
}
