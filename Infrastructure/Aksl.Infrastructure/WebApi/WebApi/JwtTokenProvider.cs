
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Security.Claims;
using System.Security.Policy;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Resources;
using System.Xml;
using System.Xml.Linq;

namespace Aksl.Infrastructure;

public class JwtTokenProvider
{
    #region Members
    private WebApiProvider _webApiProvider;
    private ILogger<JwtTokenProvider> _logger;
    #endregion

    #region Constructors
    public JwtTokenProvider( WebApiProvider webApiProvider, ILogger<JwtTokenProvider> logger)
    {
        _webApiProvider = webApiProvider;
        _logger = logger ?? NullLoggerFactory.Instance.CreateLogger<JwtTokenProvider>();
    }
    #endregion

    #region Properties
    public string AccessToken { get; set; }

    public string RefreshToken { get; set; }
    #endregion

    #region Post Method
    public async Task<LoginResponse> GetTokenAsync(string url, string userName, string password)
    {
        //try
        //{
        // using var httpClient = _httpClientFactory.CreateClient();
        //// HttpClient httpClient = new HttpClient();
        // httpClient.DefaultRequestHeaders.Accept.Clear();
        // httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var loginResponse = await _webApiProvider.PostAsync<LoginResponse, LoginRequest>(url, new LoginRequest() { UserName = userName, Password = password });
       
        AccessToken = loginResponse.AccessToken;
        RefreshToken = loginResponse.RefreshToken;

        if (loginResponse.Succeeded && !string.IsNullOrEmpty(loginResponse.AccessToken))
        {
            //_webApiProvider.HeaderProperties.SetString("Authorization", $"Bearer {_jwtTokenProvider.AccessToken}");
            _webApiProvider.SetBearer(loginResponse.AccessToken, loginResponse.RefreshToken);
        }

        return loginResponse;
        //var loginRequestJosn = await JsonSerializerHelper.SerializeStringAsync<LoginRequest>(loginRequest);
        //HttpContent content = new StringContent(loginRequestJosn);
        //var response = await httpClient.PostAsync(url, content);

        //var response = await httpClient.PostAsJsonAsync<LoginRequest>(url, loginRequest);
        ////  response.EnsureSuccessStatusCode();
        //if (response.IsSuccessStatusCode)
        //{
        //    var stream = await response.Content.ReadAsStreamAsync();
        //    var loginResponse = await JsonSerializer.DeserializeAsync<LoginResponse>(stream);

        //    AccessToken = loginResponse.AccessToken;
        //    RefreshToken = loginResponse.RefreshToken;

        //    return true;
        //}
        //else
        //{
        //    //return false;
        //    throw new HttpRequestException(response.StatusCode.ToString());
        //}

        // _logger.LogInformation($"GenerateAccessToken:{AccessToken},GenerateRefreshToken:{RefreshToken}");
        //}
        //catch (Exception ex)
        //{
        //    _logger.LogError($"GenerateAccessToken:{ex.Message}");

        //    throw new Exception($"HttpPost:{url} Error", ex);
        //}
    }
    #endregion
}

