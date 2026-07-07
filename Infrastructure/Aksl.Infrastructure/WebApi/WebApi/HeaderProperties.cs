using System;
using System.Collections.Generic;

namespace Aksl.Infrastructure;

public class HeaderProperties
{
    #region Constructors
    public HeaderProperties(): this(null)
    {
    }

    public HeaderProperties(IDictionary<string, string> parameters)
    {
        Parameters = parameters ?? new Dictionary<string, string>(StringComparer.Ordinal);
    }
    #endregion

    #region Properties
    public IDictionary<string, string> Parameters { get; set; }
    #endregion

    #region Properties
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
}
