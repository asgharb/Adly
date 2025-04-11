using Adly.Application.Common;
using Adly.Application.Contracts.User;
using Adly.Application.Extensions;
using Adly.Domain.Entities.User;
using Mediator;

namespace Adly.Application.Features.User.Commands.Register;

public class RegisterUserCommandHandler(IUserManager userManager)
    : IRequestHandler<RegisterUserCommand, OperationResult<bool>>
{
    public async ValueTask<OperationResult<bool>> Handle(RegisterUserCommand request,
        CancellationToken cancellationToken)
    {
        var user = new UserEntity(request.FirstName, request.LastName, request.UserName, request.Email)
        {
            PhoneNumber = request.PhoneNumber
        };

        var userCreateResult = await userManager.PasswordCreateAsync(user,request.Password, cancellationToken);

        if (userCreateResult.Succeeded)
            return OperationResult<bool>.SuccessResult(true); //TODO Send Confirmation Email

        return OperationResult<bool>.FailureResult(userCreateResult.Errors.ConvertToKeyValuePair());
    }
}