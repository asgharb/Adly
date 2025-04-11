using Adly.Application.Common;
using Adly.Application.Extensions;
using FluentValidation;
using Mediator;

namespace Adly.Application.Features.Common;

public class ValidateRequestBehavior<TRequest,TResponse>(IValidator<TRequest> validator):IPipelineBehavior<TRequest,TResponse>
where TRequest:IRequest<TResponse> where TResponse:IOperationResult,new ()
{
    public async ValueTask<TResponse> Handle(TRequest message, CancellationToken cancellationToken, MessageHandlerDelegate<TRequest, TResponse> next)
    {
        var validationResult = await validator.ValidateAsync(message, cancellationToken);

        if (!validationResult.IsValid)
        {
            return new TResponse()
            {
                IsSuccess = false,
                IsNotFound = false,
                ErrorMessages = validationResult.Errors.ConvertToKeyValuePair()
            };
        }


        return await next(message, cancellationToken);
    }
}