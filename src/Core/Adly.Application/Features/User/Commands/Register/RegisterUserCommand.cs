using Adly.Application.Common;
using Adly.Application.Common.Validation;
using FluentValidation;
using Mediator;

namespace Adly.Application.Features.User.Commands.Register;

public record RegisterUserCommand(
    string FirstName,
    string LastName,
    string UserName,
    string Email,
    string PhoneNumber,
    string Password,
    string RepeatPassword) : IRequest<OperationResult<bool>>, IValidatableModel<RegisterUserCommand>
{
    public IValidator<RegisterUserCommand> Validate(ValidationModelBase<RegisterUserCommand> validator)
    {
        validator.RuleFor(c => c.Email)
            .NotEmpty()
            .EmailAddress();

        validator.RuleFor(c => c.Password)
            .NotEmpty();

        validator.RuleFor(c => c.RepeatPassword)
            .NotEmpty()
            .Equal(c => c.Password)
            .WithMessage("Password And Repeat Password Are Not Same");

        validator.RuleFor(c => c.LastName)
            .NotEmpty();

        validator.RuleFor(c => c.FirstName)
            .NotEmpty();

        validator.RuleFor(c => c.UserName)
            .NotEmpty();

        validator.RuleFor(c => c.Password)
            .NotEmpty();
        
        return validator;
    }
}