﻿using Adly.Application.Common;
using Adly.Application.Common.Validation;
using FluentValidation;
using Mediator;

namespace Adly.Application.Features.Location.Queries;

public record GetLocationByNameQuery(string LocationNameSearchTerm)
    : IRequest<OperationResult<List<GetLocationByNameQueryResult>>>, IValidatableModel<GetLocationByNameQuery>
{
    public IValidator<GetLocationByNameQuery> Validate(ValidationModelBase<GetLocationByNameQuery> validator)
    {
        validator.RuleFor(c => c.LocationNameSearchTerm)
            .NotEmpty()
            .MinimumLength(3);

        return validator;
    }
}