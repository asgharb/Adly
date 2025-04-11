using Adly.Application.Features.Ad.Commands;

namespace Adly.Api.Models.Ad;

public record CreateAdApiModel
(   Guid CategoryId
    , Guid LocationId
    , string Title
    , string Description
    ,CreateAdCommand.CreateAdImagesModel[] AdImages);