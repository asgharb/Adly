using AutoMapper;

namespace Adly.Application.Common.MappingConfigurations;

public interface ICreateApplicationMapper<TSource>
{
    void Map(Profile profile)
    {
        profile.CreateMap(typeof(TSource), GetType()).ReverseMap();
    }
}