
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;

namespace Aksl.Infrastructure;

public class ApiResult
{
    #region Members
    private static readonly ApiResult _success = new() { Succeeded = true };
    #endregion

    #region Constructors
    #endregion

    #region Properties
    public static ApiResult Success => _success;
    public bool Succeeded { get; set; }
    public List<string> Errors { get; set; } = new List<string>();
    #endregion

    #region Methods
    public static ApiResult Failed(params string[] errors)
    {
        ApiResult apiResult = new() { Succeeded = false };
        if (errors is not null)
        {
            apiResult.Errors.AddRange(errors);
        }
        return apiResult;
    }

    public static ApiResult Failed(List<string>? errors)
    {
        ApiResult apiResult = new() { Succeeded = false };
        if (errors is not null)
        {
            apiResult.Errors.AddRange(errors);
        }
        return apiResult;
    }

    public override string ToString()
    {
        return Succeeded ? "Succeeded" :
               string.Format(CultureInfo.InvariantCulture, "{0} : {1}", "Failed", string.Join(",", Errors.Select(x => x).ToList()));
    }
    #endregion
}
