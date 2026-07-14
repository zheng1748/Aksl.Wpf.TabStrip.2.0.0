using Aksl.Dialogs.Services;
using Aksl.Toolkit.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Prism;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using Prism.Unity;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Interop;
using Unity;

namespace Aksl.Infrastructure;

public class LoginHandler
{
    #region Members
    private ILogger<LoginHandler> _logger;
    private WebApiAddressSettings _webApiAddressSettings;
    #endregion

    #region Constructors
    public LoginHandler(WebApiProvider webApiProvider, IOptions<WebApiAddressSettings> webApiAddressOption, ILogger<LoginHandler> logger)
    {
        WebApiProvider = webApiProvider;
        _webApiAddressSettings = webApiAddressOption.Value;
        _logger = logger;

        BuildActions();
    }
    #endregion

    #region Properties
    public WebApiProvider WebApiProvider { get; set; }
    public Action<string, string> BindAccessTokenAction { get; set; }
    public Func<string, string, Task<LoginResponse>> ExecuteLoginAction { get; set; }
    public Func<string, Task<LoginOutResponse>> ExecuteLoginOutAction { get; set; }
    public Func<string, string, Task<RefreshTokenResponse>> ExecuteRefreshTokenAction { get; set; }
    public Func<string, Task<ResetLockoutResponse>> ExecuteResetLockoutAction { get; set; }
    public Func<HttpQueryKeyValuePair[], Task<GenerateEmailTokenResponse>> ExecuteGetEmailConfirmationTokenAction { get; set; }
    #endregion

    #region Build Action Method
    public void BuildActions()
    {
        BindAccessTokenAction = (ak, rk) => WebApiProvider.SetBearer(ak, rk);
        ExecuteLoginAction = LoginAsync;
        ExecuteLoginOutAction = LoginOutAsync;
        ExecuteRefreshTokenAction = RefreshTokenAsync;
        ExecuteResetLockoutAction = ResetLockoutAsync;
        ExecuteGetEmailConfirmationTokenAction = GetEmailConfirmationTokenAsync;
    }
    #endregion

    #region Login Method
    public async Task<LoginResponse> LoginAsync(string userName, string password)
    {
        var loginResponse = await WebApiProvider.PostAsync<LoginResponse, LoginRequest>(_webApiAddressSettings.LoginUrl,
                                        new LoginRequest() { UserName = userName, Password = password, RefreshToken = WebApiProvider.RefreshToken });

        if (loginResponse.Succeeded && !string.IsNullOrEmpty(loginResponse.AccessToken))
        {
            var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(loginResponse.AccessToken);
            //WebApiProvider.AccessTokenExpire = jwtToken.ValidTo.ToUniversalTime();

            long accessTokenExpiryDate = long.Parse(jwtToken.Claims.FirstOrDefault(static x => x.Type == JwtRegisteredClaimNames.Exp).Value);
            var accessTokenUtcExpiryDate = Toolkit.DateTimeExtensions.UnixTimeStampToDateTime(accessTokenExpiryDate);
            var refreshTokenUtcExpire = Toolkit.DateTimeExtensions.ConvertToDateTime(long.Parse(loginResponse.RefreshTokenExpire));

            var accessTokenExpire = DateTime.UtcNow - accessTokenUtcExpiryDate;
            var refreshTokenExpire = DateTime.UtcNow - refreshTokenUtcExpire;

            WebApiProvider.AccessTokenExpire = accessTokenUtcExpiryDate;
            WebApiProvider.RefreshTokenExpire = refreshTokenUtcExpire;
            WebApiProvider.SetBearer(loginResponse.AccessToken, loginResponse.RefreshToken);

            // BindAccessTokenAction(loginResponse.AccessToken, loginResponse.RefreshToken);

            _logger.LogInformation($"Execute Login Method AccessToken :{loginResponse.AccessToken} AccessTokenExpire :{accessTokenExpire} RefreshToken :{loginResponse.RefreshToken} From {_webApiAddressSettings.LoginUrl}");
        }
        else
        {
            //WebApiProvider.ClearHeader();

            _logger.LogInformation($"Execute Login Method Failure:{loginResponse.ToString()} From {_webApiAddressSettings.LoginUrl}");
        }

        return loginResponse;
    }
    #endregion

    #region LoginOut Method
    public async Task<LoginOutResponse> LoginOutAsync(string userName)
    {
        var loginOutResponse = await WebApiProvider.PostAsync<LoginOutResponse, LoginOutRequest>(_webApiAddressSettings.LoginOutUrl,
                              new LoginOutRequest() { UserName = userName, AccessToken = WebApiProvider.AccessToken, RefreshToken = WebApiProvider.RefreshToken });

        if (loginOutResponse.Succeeded)
        {
            //BindAccessTokenAction(null, null);
            WebApiProvider.ClearHeader();

            _logger.LogInformation($"Execute LoginOut Method Succeeded From {_webApiAddressSettings.LoginUrl}");
        }
        else
        {
            WebApiProvider.ClearHeader();

            _logger.LogInformation($"Execute Login Method Failure:{loginOutResponse.ToString()} From {_webApiAddressSettings.LoginUrl}");
        }

        return loginOutResponse;
    }
    #endregion

    #region RefreshToken Method
    public async Task<RefreshTokenResponse> RefreshTokenAsync(string accessToken, string refreshToken)
    {
        var refreshTokenResponse = await WebApiProvider.
                 PostAsync<RefreshTokenResponse, RefreshTokenRequest>(_webApiAddressSettings.RefreshTokenUrl, new RefreshTokenRequest(accessToken, refreshToken));

        if (refreshTokenResponse.Succeeded && !string.IsNullOrEmpty(refreshTokenResponse.AccessToken))
        {
            var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(refreshTokenResponse.AccessToken);

            long accessTokenExpiryDate = long.Parse(jwtToken.Claims.FirstOrDefault(static x => x.Type == JwtRegisteredClaimNames.Exp).Value);
            var accessTokenUtcExpiryDate = Toolkit.DateTimeExtensions.UnixTimeStampToDateTime(accessTokenExpiryDate);
            var refreshTokenUtcExpire = Toolkit.DateTimeExtensions.ConvertToDateTime(long.Parse(refreshTokenResponse.RefreshTokenExpire));

            WebApiProvider.AccessTokenExpire = accessTokenUtcExpiryDate;
            WebApiProvider.RefreshTokenExpire = refreshTokenUtcExpire;
            WebApiProvider.SetBearer(refreshTokenResponse.AccessToken, refreshTokenResponse.RefreshToken);

            //  BindAccessTokenAction(refreshTokenResponse.AccessToken, refreshTokenResponse.RefreshToken);

            _logger.LogInformation($"Execute RefreshToken Method AccessToken :{refreshTokenResponse.AccessToken} RefreshToken :{refreshTokenResponse.RefreshToken} From {_webApiAddressSettings.RefreshTokenUrl}");
        }
        else
        {
            _logger.LogInformation($"Execute RefreshToken Method Errors : {refreshTokenResponse.ToString()} From {_webApiAddressSettings.RefreshTokenUrl}");
        }

        return refreshTokenResponse;
    }
    #endregion

    #region ResetLockout Method
    public async Task<ResetLockoutResponse> ResetLockoutAsync(string userName)
    {
        var resetLockoutResponse = await WebApiProvider.PostAsync<ResetLockoutResponse, ResetLockoutRequest>(_webApiAddressSettings.ResetLockoutUrl, new ResetLockoutRequest() { UserName = userName });

        if (resetLockoutResponse.Succeeded)
        {
            _logger.LogInformation($"Execute ResetLock Method Succeeded From {_webApiAddressSettings.LoginUrl}");
        }
        else
        {
            _logger.LogInformation($"Execute ResetLock Method Failure:{resetLockoutResponse.ToString()} From {_webApiAddressSettings.LoginUrl}");
        }

        return resetLockoutResponse;
    }
    #endregion

    #region GetEmailConfirmationToken Method
    public async Task<GenerateEmailTokenResponse> GetEmailConfirmationTokenAsync(HttpQueryKeyValuePair[] parameters)
    {
        var getEmailConfirmationTokenUrl = HttpQueryParameter.Query(_webApiAddressSettings.GetEmailConfirmationTokenUrl, parameters).ToString();

        var generateEmailTokenResponse = await WebApiProvider.GetAsync<GenerateEmailTokenResponse>(getEmailConfirmationTokenUrl);

        if (generateEmailTokenResponse.Succeeded)
        {
            _logger.LogInformation($"Execute GetEmailConfirmationToken Method Succeeded From {_webApiAddressSettings.LoginUrl}");
        }
        else
        {
            _logger.LogInformation($"Execute Login Method Failure:{generateEmailTokenResponse.ToString()} From {_webApiAddressSettings.LoginUrl}");
        }

        return generateEmailTokenResponse;
    }
    #endregion
}

public static class LoginHandlerExtensions
{
    public static void AddLoginHandler(this IServiceCollection services)
    {
        services.AddHttpClient("WebApi");

        services.AddSingleton<WebApiProvider>();

        services.AddSingleton<LoginHandler>();
    }
}

