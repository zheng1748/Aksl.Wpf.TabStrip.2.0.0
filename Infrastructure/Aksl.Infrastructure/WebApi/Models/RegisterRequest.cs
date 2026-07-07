using System.ComponentModel.DataAnnotations;

namespace Aksl.Infrastructure;

public class RegisterRequest 
{
    [Required]
    [StringLength(maximumLength: 16, MinimumLength = 1)]
    public string UserName { get; set; } = null!;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    [StringLength(maximumLength: 16, MinimumLength = 8)]
    public string Password { get; set; }
}

public class RegisterResponse : ApiResult
{
    public string UserId { get; set; }

    public string AccessToken { get; set; }

    public string RefreshToken { get; set; }
}
