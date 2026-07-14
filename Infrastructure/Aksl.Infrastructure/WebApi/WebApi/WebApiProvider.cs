using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Security.Policy;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Resources;
using System.Xml;

namespace Aksl.Infrastructure;

//Wrapper
public class WebApiProvider
{
    #region Members
    private IHttpClientFactory _httpClientFactory;
    private ILogger<WebApiProvider> _logger;
    #endregion

    #region Constructors
    public WebApiProvider(IHttpClientFactory httpClientFactory, ILogger<WebApiProvider> logger)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentException("HttpClientFactory is not null");
        _logger = logger ?? NullLoggerFactory.Instance.CreateLogger<WebApiProvider>();

        HeaderProperties = new();
    }
    #endregion

    #region Properties
    public HeaderProperties HeaderProperties { get; set; }

    public void SetBearer(string accessToken, string refreshToken)
    {
        Debug.Assert(AccessToken != accessToken);

        AccessToken = accessToken;
        RefreshToken = refreshToken;

        SetHeader("Authorization", $"Bearer {accessToken}");
    }

    public void SetHeader(string key, string value)
    {
        if (HeaderProperties.Parameters.ContainsKey(key))
        {
            HeaderProperties.Parameters.Remove(key);
        }
        HeaderProperties.SetString(key, value);
    }

    public void ClearHeader()
    {
        AccessToken = null; AccessTokenExpire = null;
        RefreshToken = null; RefreshTokenExpire = null;

        HeaderProperties.Parameters.Clear();
    }

    public string? AccessToken { get; set; }
    public DateTime? AccessTokenExpire { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpire { get; set; }
    public bool IsAccessTokenExpired
    {
        get
        {
            return DateTime.UtcNow > AccessTokenExpire;
        }
    }
    public bool IsRefreshTokenExpire
    {
        get
        {
            return DateTime.UtcNow > RefreshTokenExpire;
        }
    }
    #endregion

    #region Get Method
    public async Task<TResponse> GetAsync<TResponse>(string requestUrl, int timeoutSecond = 180, CancellationToken cancellationToken = default)
    {
        var httpClient = CreateHttpClient(HeaderProperties, timeoutSecond);

        var response = await httpClient.GetAsync(requestUrl);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<TResponse>();
        }
        else
        {
            _logger.LogError($"HttpGet:{requestUrl} StatusCode:{response.StatusCode}");

            return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken);
        }
    }
    #endregion

    #region Post Method
    public async Task<TResponse> PostAsync<TResponse, TValue>(string requestUrl, TValue value, int timeoutSecond = 180, CancellationToken cancellationToken = default)
    {
        var httpClient = CreateHttpClient(HeaderProperties, timeoutSecond);
        var response = await httpClient.PostAsJsonAsync<TValue>(requestUrl, value, cancellationToken);

        //response.EnsureSuccessStatusCode();
        // return await response.Content.ReadFromJsonAsync<T>(cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            //var stream = await response.Content.ReadAsStreamAsync();
            //var loginResponse = await JsonSerializer.DeserializeAsync<V>(stream);

            return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken);
        }
        else
        {
            //var apiResult = await response.Content.ReadFromJsonAsync<T>(cancellationToken) as ApiResult;
            //throw new HttpRequestException($"Error:{apiResult.ToString()}");

            _logger.LogError($"HttpGet:{requestUrl} StatusCode:{response.StatusCode}");

            return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken);
        }
    }
    #endregion

    #region Put Method
    public async Task<TResponse> PutAsync<TResponse, TValue>(string requestUrl, TValue value, int timeoutSecond = 180, CancellationToken cancellationToken = default)
    {
        var httpClient = CreateHttpClient(HeaderProperties, timeoutSecond);
        var response = await httpClient.PutAsJsonAsync<TValue>(requestUrl, value, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken);
        }
        else
        {
            _logger.LogError($"HttpGet:{requestUrl} StatusCode:{response.StatusCode}");

            return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken);
        }
    }
    #endregion

    #region Delete Method
    public async Task<TResponse> DeleteAsync<TResponse, TValue>(string requestUrl, TValue value, int timeoutSecond = 180, CancellationToken cancellationToken = default)
    {
        var httpClient = CreateHttpClient(HeaderProperties, timeoutSecond);
        var response = await httpClient.DeleteAsync(requestUrl, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken);
        }
        else
        {
            _logger.LogError($"HttpGet:{requestUrl} StatusCode:{response.StatusCode}");

            return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken);
        }
    }
    #endregion

    #region CreateHttpClient Methods
    private HttpClient CreateHttpClient(HeaderProperties headerProperties = null, int? timeoutSecond = null)
    {
        var httpClient = _httpClientFactory.CreateClient("WebApi");
        httpClient.DefaultRequestHeaders.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        if (headerProperties is not null && headerProperties.Parameters.Any())
        {
            foreach (var headerItem in headerProperties.Parameters)
            {
                if (!httpClient.DefaultRequestHeaders.Contains(headerItem.Key))
                {
                    httpClient.DefaultRequestHeaders.Add(headerItem.Key, headerItem.Value);
                }
            }
        }

        if (timeoutSecond is not null)
        {
            httpClient.Timeout = TimeSpan.FromSeconds(timeoutSecond.Value);
        }

        return httpClient;
    }
    #endregion

    #region Get ResourceStream Method
    private StringContent GenerateStringContent(string requestBody, Dictionary<string, string> dicHeaders)
    {
        var content = new StringContent(requestBody);
        if (dicHeaders != null)
        {
            foreach (var headerItem in dicHeaders)
            {
                content.Headers.Add(headerItem.Key, headerItem.Value);
            }
        }
        return content;
    }
    #endregion
}

