using Adly.Application.Common;
using Adly.Application.Common.Validation;
using FluentValidation;
using Mediator;

namespace Adly.Application.Features.Location.Queries;

public record GetLocationByIdQuery(Guid LocationId) : IRequest<OperationResult<GetLocationByIdQueryResult>>,
    IValidatableModel<GetLocationByIdQuery>
{
    public IValidator<GetLocationByIdQuery> Validate(ValidationModelBase<GetLocationByIdQuery> validator)
    {
        validator.RuleFor(c => c.LocationId)
            .NotEmpty();

        return validator;
    }
}