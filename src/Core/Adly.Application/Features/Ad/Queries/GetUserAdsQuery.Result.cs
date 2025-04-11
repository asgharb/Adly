using Adly.Domain.Entities.Ad;

namespace Adly.Application.Features.Ad.Queries;

public record GetUserAdsQueryResult(Guid AdId,string Title,DateTime ModifiedDate,AdEntity.AdStates CurrentState);