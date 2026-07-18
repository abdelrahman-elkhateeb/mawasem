using Mawasem.Application.Features.Collections.Interfaces;
using Mawasem.Infrastructure.Persistence.Contexts;

namespace Mawasem.Infrastructure.Collections;

public sealed partial class CollectionManagementService
    : ICollectionManagementService
{
    private const int MaximumPageSize = 100;

    private const int MaximumSearchLength = 256;

    private const int MaximumNameLength = 100;

    private readonly MawasemDbContext _dbContext;

    private readonly TimeProvider _timeProvider;

    public CollectionManagementService(
        MawasemDbContext dbContext ,
        TimeProvider timeProvider )
    {
        _dbContext = dbContext;
        _timeProvider = timeProvider;
    }
}