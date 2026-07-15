using Mawasem.Application.Features.Categories.Interfaces;
using Mawasem.Infrastructure.Persistence.Contexts;

namespace Mawasem.Infrastructure.Categories;

public sealed partial class CategoryManagementService
    : ICategoryManagementService
{
    private const int MaximumPageSize = 100;

    private const int MaximumSearchLength = 256;

    private const int MaximumNameLength = 100;

    private readonly MawasemDbContext _dbContext;

    private readonly TimeProvider _timeProvider;

    public CategoryManagementService(
        MawasemDbContext dbContext ,
        TimeProvider timeProvider )
    {
        _dbContext = dbContext;
        _timeProvider = timeProvider;
    }
}