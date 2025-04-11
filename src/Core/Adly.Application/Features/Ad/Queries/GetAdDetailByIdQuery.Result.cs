using Adly.Application.Common.MappingConfigurations;
using Adly.Domain.Entities.Ad;
using AutoMapper;

namespace Adly.Application.Features.Ad.Queries;

public record GetAdDetailByIdQueryResult(
    Guid AdId,
    string Title,
    AdEntity.AdStates CurrentState,
    string Description,
    string LocationName,
    Guid LocationId,
    string CategoryName,
    Guid CategoryId,
    Guid OwnerId,
    string OwnerUserName,
    string OwnerPhoneNumber):ICreateApplicationMapper<AdEntity>
{
    public record AdDetailImageModel(string ImageName, string ImageUrl);
    public AdDetailImageModel[] AdImages { get; set; }


    public void Map(Profile profile)
    {
        profile.CreateMap< AdEntity,GetAdDetailByIdQueryResult>()

            .ForCtorParam(nameof(AdId),opt=>opt.MapFrom(c=>c.Id))
            .ForCtorParam(nameof(Title),opt=>opt.MapFrom(c=>c.Title))
            .ForCtorParam(nameof(Description),opt=>opt.MapFrom(c=>c.Description))
            .ForCtorParam(nameof(LocationName),opt=>opt.MapFrom(c=>c.Location.Name))
            .ForCtorParam(nameof(LocationId),opt=>opt.MapFrom(c=>c.LocationId))
            .ForCtorParam(nameof(CategoryName),opt=>opt.MapFrom(c=>c.Category.Name))
            .ForCtorParam(nameof(CategoryId),opt=>opt.MapFrom(c=>c.CategoryId))
            .ForCtorParam(nameof(OwnerId),opt=>opt.MapFrom(c=>c.UserId))
            .ForCtorParam(nameof(OwnerUserName),opt=>opt.MapFrom(c=>c.User.UserName))
            .ForCtorParam(nameof(OwnerPhoneNumber),opt=>opt.MapFrom(c=>c.User.PhoneNumber))
            .ForCtorParam(nameof(CurrentState),opt=>opt.MapFrom(c=>c.CurrentState))
            .ReverseMap();
    }
}