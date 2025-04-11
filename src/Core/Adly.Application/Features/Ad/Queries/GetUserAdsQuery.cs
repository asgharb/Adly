using Adly.Application.Common;
using Adly.Application.Common.Validation;
using Adly.Domain.Entities.Ad;
using FluentValidation;
using Mediator;

namespace Adly.Application.Features.Ad.Queries;

public record GetUserAdsQuery(Guid UserId)
    : IValidatableModel<GetUserAdsQuery>, IRequest<OperationResult<List<GetUserAdsQueryResult>>>
{
    public IValidator<GetUserAdsQuery> Validate(ValidationModelBase<GetUserAdsQuery> validator)
    {
        validator.RuleFor(c => c.UserId)
            .NotEmpty();

        return validator;
    }
}