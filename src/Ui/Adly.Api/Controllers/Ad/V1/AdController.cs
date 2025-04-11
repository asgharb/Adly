using Adly.Api.Models.Ad;
using Adly.Application.Features.Ad.Commands;
using Adly.Application.Features.Ad.Queries;
using Adly.WebFramework.Common;
using Adly.WebFramework.Models;
using Asp.Versioning;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Adly.Api.Controllers.Ad.V1;

[ApiController]
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/ad")]
[Authorize]
public class AdController(ISender sender) : BaseController
{
    /// <summary>
    /// Creates a 
    /// </summary>
    /// <returns></returns>
    [HttpPost("Create")]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create(CreateAdApiModel model, CancellationToken cancellationToken)
        => base.OperationResult(await sender.Send(
            new CreateAdCommand(base.UserId!.Value, model.CategoryId, model.LocationId, model.Title, model.Description,
                model.AdImages), cancellationToken));
    
    /// <summary>
    /// Gets a list of user ads
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("UserAds")]
    [ProducesResponseType(typeof(ApiResult<List<GetUserAdsQueryResult>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserAds(CancellationToken cancellationToken)
    =>base.OperationResult(await sender.Send(new GetUserAdsQuery(base.UserId!.Value), cancellationToken));
    
    /// <summary>
    /// Edits an ad 
    /// </summary>
    /// <param name="model"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut("EditAd")]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> EditAd(EditAdCommand model, CancellationToken cancellationToken)
    =>base.OperationResult(await sender.Send(model, cancellationToken));
    
    [HttpGet("Detail")]
    [ProducesResponseType(typeof(ApiResult<GetAdDetailByIdQueryResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAdDetailById(Guid id, CancellationToken cancellationToken)
    => base.OperationResult(await sender.Send(new GetAdDetailByIdQuery(id), cancellationToken));
}