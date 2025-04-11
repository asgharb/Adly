using Adly.Application.Features.Category.Commands;
using Adly.WebFramework.Common;
using Adly.WebFramework.Models;
using Asp.Versioning;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Adly.Api.Controllers.Admin.V1;

[ApiController]
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/admin/category")]
public class CategoryController(ISender sender) : BaseController
{
    /// <summary>
    /// Creates specific location
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> CreateCategory(CreateCategoryCommand model, CancellationToken cancellationToken)
        => base.OperationResult(await sender.Send(model, cancellationToken));
}