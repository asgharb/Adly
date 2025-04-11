using Adly.Application.Common;
using Adly.Application.Common.Validation;
using FluentValidation;
using Mediator;

namespace Adly.Application.Features.Ad.Commands;

public record CreateAdCommand(Guid UserId
    , Guid CategoryId
    , Guid LocationId
    , string Title
    , string Description
    ,CreateAdCommand.CreateAdImagesModel[] AdImages):IRequest<OperationResult<bool>>
,IValidatableModel<CreateAdCommand>
{
    public record CreateAdImagesModel(string Base64File,string FileContent);

    public IValidator<CreateAdCommand> Validate(ValidationModelBase<CreateAdCommand> validator)
    {
        validator.RuleFor(c => c.UserId)
            .NotEmpty();

        validator.RuleFor(c => c.CategoryId)
            .NotEmpty();

        validator.RuleFor(c => c.LocationId)
            .NotEmpty();

        validator.RuleFor(c => c.Description)
            .NotEmpty();

        validator.RuleFor(c => c.Title)
            .NotEmpty();

        return validator;
    }
}

