using Adly.Application.Common;
using Adly.Application.Contracts.FileService.Interfaces;
using Adly.Application.Repositories.Common;
using Adly.Domain.Entities.Ad;
using AutoMapper;
using Mediator;

namespace Adly.Application.Features.Ad.Queries;

public class GetAdDetailByIdQueryHandler(IUnitOfWork unitOfWork,IFileService fileService,IMapper mapper)
:IRequestHandler<GetAdDetailByIdQuery,OperationResult<GetAdDetailByIdQueryResult>>
{
    public async ValueTask<OperationResult<GetAdDetailByIdQueryResult>> Handle(GetAdDetailByIdQuery request, CancellationToken cancellationToken)
    {
        var ad = await unitOfWork.AdRepository.GetAdDetailByIdAsync(request.AdId, cancellationToken);
        
        if(ad is null)
            return OperationResult<GetAdDetailByIdQueryResult>.FailureResult(nameof(GetAdDetailByIdQuery.AdId),"Specified Ad not found");

        var adImages =
            await fileService.GetFilesByNameAsync(ad.Images.Select(c => c.FileName).ToList(), cancellationToken);

        var result = mapper.Map<AdEntity, GetAdDetailByIdQueryResult>(ad);

        result.AdImages = adImages.Select(c => new GetAdDetailByIdQueryResult.AdDetailImageModel(c.FileName, c.FileUrl))
            .ToArray();
        
        
        return OperationResult<GetAdDetailByIdQueryResult>.SuccessResult(result);

    }
}