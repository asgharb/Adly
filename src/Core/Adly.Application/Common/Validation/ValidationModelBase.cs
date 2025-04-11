using FluentValidation;

namespace Adly.Application.Common.Validation;

/// <summary>
/// Marker Validator Class
/// </summary>
/// <typeparam name="TRequestModel">A request model in form of command or query</typeparam>
public class ValidationModelBase<TRequestModel>:AbstractValidator<TRequestModel> where TRequestModel:class
{
    
}