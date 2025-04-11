﻿using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace Adly.WebFramework.Models;


public enum ApiResultStatusCode
{
    [Display(Name = "Success")]
    Ok = 200,
    [Display(Name = "NotFound")]
    Notfound = 404,
    
    [Display(Name = "BadRequest")]
    BadRequest = 400,
    
    [Display(Name = "ServerError")]
    ServerError = 500,
    
    [Display(Name = "Forbidden")]
    Forbidden = 403,
    
    [Display(Name = "Unauthorized")]
    Unauthorized = 401,
}

public record ApiResult(
    bool IsSuccess,
    string Message,
    ApiResultStatusCode StatusCode
)
{
    public string RequestId => Activity.Current?.TraceId.ToHexString() ?? string.Empty;
}

public record ApiResult<TResult>(bool IsSuccess
    ,string Message
    ,ApiResultStatusCode StatusCode
    , TResult Data)
    :ApiResult(IsSuccess,Message,StatusCode);