using Adly.Application.Features.Location.Queries;
using Adly.WebFramework.Common;
using Adly.WebFramework.Models;
using Asp.Versioning;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Adly.Api.Controllers.Location.V1;

[ApiController]
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/location")]
[Authorize]
public class LocationController(ISender sender) : BaseController
{
    /// <summary>
    /// Gets a location based on Id
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResult<List<GetLocationByIdQueryResult>>),StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetLocation(Guid locationId,CancellationToken cancellationToken)
    =>base.OperationResult(await sender.Send(new GetLocationByIdQuery(locationId), cancellationToken));

    /// <summary>
    /// Gets a list of location based on name provided
    /// </summary>
    [HttpGet("Search")]
    [ProducesResponseType(typeof(ApiResult<List<GetLocationByNameQueryResult>>),StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> SearchLocations(string name, CancellationToken cancellationToken)
        => base.OperationResult(await sender.Send(new GetLocationByNameQuery(name), cancellationToken));
}