using Adly.Application.Features.Category.Queries;
using Adly.WebFramework.Common;
using Adly.WebFramework.Models;
using Asp.Versioning;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Adly.Api.Controllers.Category.V1;

[ApiController]
[ApiVersion("1")]
[Authorize]
[Route("api/v{version:apiVersion}/category")]
public class CategoryController(ISender sender) : BaseController
{
   /// <summary>
   /// Gets a category based on Id
   /// </summary>
   /// <returns></returns>
   [HttpGet]
   [ProducesResponseType(typeof(ApiResult<GetCategoryByIdQueryResult>),StatusCodes.Status200OK)]
   public virtual async Task<IActionResult> GetCategory(Guid categoryId,CancellationToken cancellationToken)
   =>base.OperationResult(await sender.Send(new GetCategoryByIdQuery(categoryId), cancellationToken));
   
   /// <summary>
   /// Gets a list of category based on given name
   /// </summary>
   /// <returns></returns>
   [HttpGet("Search")]
   [ProducesResponseType(typeof(ApiResult<List<GetCategoriesByNameQueryResult>>), StatusCodes.Status200OK)]
   public virtual async Task<IActionResult> SearchCategory(string categoryName,CancellationToken cancellationToken)
   =>base.OperationResult(await sender.Send(new GetCategoriesByNameQuery(categoryName),cancellationToken));
   
}