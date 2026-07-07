using System.ComponentModel.DataAnnotations;

namespace Aksl.Infrastructure;

public class RoleRequest 
{
    [Required]
    [StringLength(maximumLength: 16, MinimumLength = 1)]
    public string Name { get; set; } 
}
