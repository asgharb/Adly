using Adly.Application.Common;
using Adly.Application.Common.Validation;
using FluentValidation;
using Mediator;

namespace Adly.Application.Features.Category.Queries;

public record GetCategoryByIdQuery(Guid CategoryId) : IRequest<OperationResult<GetCategoryByIdQueryResult>>,
    IValidatableModel<GetCategoryByIdQuery>
{
    public IValidator<GetCategoryByIdQuery> Validate(ValidationModelBase<GetCategoryByIdQuery> validator)
    {
        validator.RuleFor(c => c.CategoryId)
            .NotEmpty();

        return validator;
    }
}