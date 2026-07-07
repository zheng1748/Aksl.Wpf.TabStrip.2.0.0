using System.ComponentModel.DataAnnotations;

namespace Aksl.Infrastructure;

public record GenerateEmailTokenRequest
{
    //[Required]
    //[StringLength(maximumLength:16, MinimumLength = 1)]
    //public string UserName { get; set; } 

    [Required]
    [EmailAddress]
    public string Email { get; set; }
}

public class GenerateEmailTokenResponse : ApiResult
{
    public string Token { get; set; }
}

public class ConfirmEmailRequest
{
    //[Required]
    //[StringLength(maximumLength: 16, MinimumLength = 1)]
    //public string UserName { get; set; }
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    [Required]
    public string Token { get; set; }
}

public class ConfirmEmailResponse : ApiResult
{
    public bool Succeeded { get; set; }
}

