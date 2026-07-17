using Mawasem.Application.Features.Brands.Interfaces;
using Mawasem.Infrastructure.Persistence.Contexts;

namespace Mawasem.Infrastructure.Brands;

public sealed partial class BrandManagementService
    : IBrandManagementService
{
    private const int MaximumPageSize = 100;

    private const int MaximumSearchLength = 256;

    private const int MaximumNameLength = 100;

    private const int MaximumDescriptionLength = 500;

    private const int MaximumLogoUrlLength = 500;

    private readonly MawasemDbContext _dbContext;

    private readonly TimeProvider _timeProvider;

    public BrandManagementService(
        MawasemDbContext dbContext ,
        TimeProvider timeProvider )
    {
        _dbContext = dbContext;
        _timeProvider = timeProvider;
    }
}