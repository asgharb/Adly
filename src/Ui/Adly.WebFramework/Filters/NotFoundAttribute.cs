using Adly.WebFramework.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Adly.WebFramework.Filters;

public class NotFoundAttribute:ResultFilterAttribute
{
    public override void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.Result is NotFoundObjectResult notFoundObjectResult)
        {
            var apiResult=new ApiResult<object>
            (false,ApiResultStatusCode.Notfound.ToString("G")
                , ApiResultStatusCode.Notfound,
                notFoundObjectResult.Value!);

            context.Result = new JsonResult(apiResult) { StatusCode = StatusCodes.Status404NotFound };
        }

        if (context.Result is NotFoundResult notFoundResult)
        {
            var apiResult = new ApiResult(false, ApiResultStatusCode.Notfound.ToString("G"),
                ApiResultStatusCode.Notfound);
            
            context.Result=new JsonResult(apiResult){StatusCode = StatusCodes.Status404NotFound};
        }
    }
}