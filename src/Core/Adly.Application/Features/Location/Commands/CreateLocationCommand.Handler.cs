using Adly.Application.Common;
using Adly.Application.Repositories.Common;
using Adly.Domain.Entities.Ad;
using Mediator;

namespace Adly.Application.Features.Location.Commands;

public class CreateLocationCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<CreateLocationCommand, OperationResult<bool>>
{
    public async ValueTask<OperationResult<bool>> Handle(CreateLocationCommand request,
        CancellationToken cancellationToken)
    {
        if (await unitOfWork.LocationRepository.IsLocationNameExistsAsync(request.LocationName, cancellationToken))
            return OperationResult<bool>.FailureResult(nameof(CreateLocationCommand.LocationName),
                "This location name already exists");

        var location = new LocationEntity(request.LocationName);

        await unitOfWork.LocationRepository.CreateAsync(location, cancellationToken);

        await unitOfWork.CommitAsync(cancellationToken);
        
        return OperationResult<bool>.SuccessResult(true);
    }
}