using Adly.Application.Common;
using Adly.Application.Repositories.Common;
using Mediator;

namespace Adly.Application.Features.Ad.Queries;

public class GetUserAdsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetUserAdsQuery, OperationResult<List<GetUserAdsQueryResult>>>
{
    public async ValueTask<OperationResult<List<GetUserAdsQueryResult>>> Handle(GetUserAdsQuery request,
        CancellationToken cancellationToken)
    {
        var userAds = await unitOfWork.AdRepository.GetUserAdsAsync(request.UserId, cancellationToken);

        var result = userAds.Select(c =>
            new GetUserAdsQueryResult(c.Id, c.Title, c.ModifiedDate ?? c.CreatedDate, c.CurrentState)).ToList();
        
        
        return OperationResult<List<GetUserAdsQueryResult>>.SuccessResult(result);
    }
}