using Mawasem.Application.Features.Seasons.Interfaces;
using Mawasem.Infrastructure.Persistence.Contexts;

namespace Mawasem.Infrastructure.Seasons;

public sealed partial class SeasonManagementService
    : ISeasonManagementService
{
    private const int MaximumPageSize = 100;

    private const int MaximumSearchLength = 256;

    private const int MaximumNameLength = 100;

    private const int MaximumDescriptionLength = 500;

    private readonly MawasemDbContext _dbContext;

    private readonly TimeProvider _timeProvider;

    public SeasonManagementService(
        MawasemDbContext dbContext ,
        TimeProvider timeProvider )
    {
        _dbContext = dbContext;
        _timeProvider = timeProvider;
    }
}