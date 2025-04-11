using Adly.Application.Common.MappingConfigurations;
using Adly.Application.Common.Validation;
using Adly.Application.Features.Common;
using FluentValidation;
using Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace Adly.Application.Extensions;

public static class ApplicationServiceCollectionExtension
{
    public static IServiceCollection RegisterApplicationValidators(this IServiceCollection services)
    {

        var validationTypes = AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(c => c.GetExportedTypes())
            .Where(c => c.GetInterfaces().Any(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValidatableModel<>))).ToList();


        foreach (var validationType in validationTypes)
        {
            var biggestConstructorLength = validationType.GetConstructors()
                .OrderByDescending(c=>c.GetParameters().Length).First().GetParameters().Length;

            var requestModel = Activator.CreateInstance(validationType, new object[biggestConstructorLength]);
            
            if(requestModel is null)
                continue;

            var requestMethodInfo = validationType.GetMethod(nameof(IValidatableModel<object>.Validate));
            var validationModelBase =
                Activator.CreateInstance(typeof(ValidationModelBase<>).MakeGenericType(validationType));
            
           if(validationModelBase is null)
               continue;


           var validator = requestMethodInfo?.Invoke(requestModel, new[] { validationModelBase });
           
           if(validator is null)
               continue;

           var validatorInterface = validator.GetType()
               .GetInterfaces()
               .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValidator<>));
           
           if(validatorInterface is null)
               continue;

           services.AddTransient(validatorInterface, _ => validator);
        }
        
        return services;
    }

    public static IServiceCollection AddApplicationAutomapper(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(RegisterApplicationMappers));

        return services;
    }

    public static IServiceCollection AddApplicationMediatorServices(this IServiceCollection services)
    {
        services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Transient;
            options.Namespace = "Adly.Application.GeneratedMediatorServices";
        });

        services.AddTransient(typeof(IPipelineBehavior<,>),typeof(ValidateRequestBehavior<,>));

        return services;
    }
}