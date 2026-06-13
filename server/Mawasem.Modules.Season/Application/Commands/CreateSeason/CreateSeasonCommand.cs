using Mawasem.Shared.Common;
using Mawasem.Shared.Enums;
using MediatR;

namespace Mawasem.Modules.Season.Application.Commands.CreateSeason;

public record CreateSeasonCommand(
    string Name ,
    SeasonType Type ,
    DateTime StartDate ,
    DateTime EndDate ,
    string? Description ,
    string? BannerImageUrl
) : IRequest<Result<Guid>>;