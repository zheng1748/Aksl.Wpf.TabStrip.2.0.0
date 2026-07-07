using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Input;

namespace Aksl.Infrastructure;

public class HttpQueryParameter
{
    #region Constructors
    public HttpQueryParameter(string baseAddress) 
    {
        BaseAddress=baseAddress;
        Parameters =  new Dictionary<string, string>(StringComparer.Ordinal);
    }

    public HttpQueryParameter()
    {
        Parameters = new Dictionary<string, string>(StringComparer.Ordinal);
    }
    #endregion

    #region Properties
    public string BaseAddress { get; set; }
    public Dictionary<string, string> Parameters { get; set; }
    #endregion

    #region Methods
    public string? GetString(string key)
    {
        return Parameters.TryGetValue(key, out var value) ? value : null;
    }

    public void SetString(string key, string? value)
    {
        if (value is not null)
        {
            Parameters[key] = value;
        }
        else
        {
            Parameters.Remove(key);
        }
    }
    #endregion

    #region Methods
    public static HttpQueryParameter Query(string baseAddress, IDictionary<string, string> parameters)
    {
        HttpQueryParameter httpQueryParameter = new(baseAddress);
        if (parameters is not null)
        {
            foreach (var param in parameters)
            {
                httpQueryParameter.Parameters[param.Key] = param.Value;
            }
        }
        return httpQueryParameter;
    }

    public static HttpQueryParameter Query(string baseAddress, params HttpQueryKeyValuePair[] httpQueryKeyValuePairs)
    {
        HttpQueryParameter httpQueryParameter = new(baseAddress);
        if (httpQueryKeyValuePairs is not null && httpQueryKeyValuePairs.Length>0)
        {
            foreach (var  kvp in httpQueryKeyValuePairs)
            {
                httpQueryParameter.Parameters[kvp.Key] = kvp.Value;
            }
        }
        return httpQueryParameter;
    }

    public static HttpQueryParameter Query(string baseAddress,  IList<HttpQueryKeyValuePair> httpQueryKeyValuePairs)
    {
        HttpQueryParameter httpQueryParameter = new(baseAddress);
        if (httpQueryKeyValuePairs is not null && httpQueryKeyValuePairs.Any())
        {
            foreach (var kvp in httpQueryKeyValuePairs)
            {
                httpQueryParameter.Parameters[kvp.Key] = kvp.Value;
            }
        }
        return httpQueryParameter;

    }
    public override string ToString()
    {
        string parameterSuffix = string.Join("&", Parameters.Select(x => $"{x.Key}={x.Value}").ToList());
        string remoteAddress = $"{BaseAddress}?{parameterSuffix}";
        return remoteAddress;
    }
    #endregion
}

public record HttpQueryKeyValuePair
{
    public HttpQueryKeyValuePair(string key, string value)
    {
        Key = key;
        Value = value;
    }

    public string Key { get; }
    //
    public string Value { get; }
}
