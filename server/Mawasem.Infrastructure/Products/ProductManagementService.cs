using Mawasem.Application.Features.Products.Interfaces;
using Mawasem.Infrastructure.Persistence.Contexts;

namespace Mawasem.Infrastructure.Products;

public sealed partial class ProductManagementService
    : IProductManagementService
{
    private const int MaxPageSize = 100;

    private readonly MawasemDbContext _dbContext;

    private readonly TimeProvider _timeProvider;

    public ProductManagementService(
        MawasemDbContext dbContext ,
        TimeProvider timeProvider )
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _dbContext = dbContext;
        _timeProvider = timeProvider;
    }
}