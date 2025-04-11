using System.Reflection;
using AutoMapper;

namespace Adly.Application.Common.MappingConfigurations;

public class RegisterApplicationMappers : Profile
{
    public RegisterApplicationMappers()
    {
        RegisterMappingProfiles(typeof(RegisterApplicationMappers).Assembly);
    }

    private void RegisterMappingProfiles(Assembly assembly)
    {
        var mappingTypes = assembly.GetTypes().Where(t => t.GetInterfaces().Any(i =>
            i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICreateApplicationMapper<>)));

        foreach (var type in mappingTypes)
        {
            var defaultConstructorLength = type.GetConstructors()
                .OrderByDescending(c => c.GetParameters().Length)
                .First().GetParameters().Length;

            var model = Activator.CreateInstance(type, new object[defaultConstructorLength]);


            var methodInfo = type.GetMethod("Map") ?? type.GetInterface("ICreateApplicationMapper`1")!.GetMethod("Map");

            if (model is not null)
                methodInfo?.Invoke(model, [this]);
        }
    }
}