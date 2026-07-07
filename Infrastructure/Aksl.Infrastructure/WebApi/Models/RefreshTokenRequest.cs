using System.ComponentModel.DataAnnotations;

namespace Aksl.Infrastructure;

public record RefreshTokenRequest([Required] string AccessToken, [Required] string RefreshToken)
{
    //    [Required]
    //public string AccessToken { get; set; }

    //[Required]
    //public string RefreshToken { get; set; }
}

public class RefreshTokenResponse : ApiResult
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    // public long RefreshTokenExpire { get; set; }
    public string RefreshTokenExpire { get; set; }
}

