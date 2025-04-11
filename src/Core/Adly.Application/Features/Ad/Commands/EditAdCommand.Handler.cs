using Adly.Application.Common;
using Adly.Application.Contracts.FileService.Interfaces;
using Adly.Application.Contracts.FileService.Models;
using Adly.Application.Repositories.Common;
using Adly.Domain.Common.ValueObjects;
using Mediator;

namespace Adly.Application.Features.Ad.Commands;

public class EditAdCommandHandler(
    IUnitOfWork unitOfWork,
    IFileService fileService) : IRequestHandler<EditAdCommand, OperationResult<bool>>
{
    public async ValueTask<OperationResult<bool>> Handle(EditAdCommand request, CancellationToken cancellationToken)
    {
        if (request.CategoryId.HasValue && request.CategoryId != Guid.Empty)
        {
            var category =
                await unitOfWork.CategoryRepository.GetCategoryByIdAsync(request.CategoryId.Value, cancellationToken);

            if (category is null)
                return OperationResult<bool>.FailureResult(nameof(EditAdCommand.CategoryId), "Category Not Found");
        }

        if (request.LocationId.HasValue && request.LocationId != Guid.Empty)
        {
            var category =
                await unitOfWork.LocationRepository.GetLocationByIdAsync(request.LocationId.Value, cancellationToken);

            if (category is null)
                return OperationResult<bool>.FailureResult(nameof(EditAdCommand.LocationId), "Location Not Found");
        }

        var ad = await unitOfWork.AdRepository.GetAdByIdForUpdateAsync(request.AdId, cancellationToken);

        if (ad is null)
            return OperationResult<bool>.FailureResult(nameof(EditAdCommand.AdId), "Ad not found");

        ad.Edit(request.Title, request.Description, request.CategoryId, request.LocationId);


        if (request.RemovedImageNames.Any())
        {
            ad.RemoveImages(request.RemovedImageNames);
            await fileService.RemoveFilesAsync(request.RemovedImageNames, cancellationToken);
        }

        if (request.NewImages.Any())
        {
            var savedNewImages =
                await fileService.SaveFilesAsync(request.NewImages.Select(c =>
                    new SaveFileModel(c.ImageContent, c.ImageType)).ToList(), cancellationToken);

            foreach (var savedNewImage in savedNewImages)
            {
                ad.AddImage(new (savedNewImage.FileName,savedNewImage.FileType));
            }
        }

        await unitOfWork.CommitAsync(cancellationToken);
        
        return OperationResult<bool>.SuccessResult(true);
    }
}