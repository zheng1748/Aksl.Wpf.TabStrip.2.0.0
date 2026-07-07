using System;
using System.IO;
using System.Security.Policy;


namespace Aksl.Infrastructure;

public class WebApiAddressSettings
{
    #region Constructors
    public WebApiAddressSettings()
    {
    }
    #endregion

    #region Properties
    public string BaseAddress { get; set; }
    public string LoginUrl { get; set; }
    public string LoginOutUrl { get; set; }
    public string RefreshTokenUrl { get; set; }
    public string ResetLockoutUrl { get; set; }
    public string CreateUserUrl { get; set; }
    public string GetEmailConfirmationTokenUrl { get; set; }
    public string ConfirmEmailTokenUrl { get; set; }
    #endregion
}

