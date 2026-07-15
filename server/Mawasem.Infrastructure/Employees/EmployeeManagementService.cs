using Mawasem.Application.Features.Employees.Interfaces;
using Mawasem.Domain.Identity;
using Mawasem.Infrastructure.Persistence.Contexts;
using Microsoft.AspNetCore.Identity;

namespace Mawasem.Infrastructure.Employees;

public sealed partial class EmployeeManagementService
    : IEmployeeManagementService
{
    private const int MaximumPageSize = 100;

    private const int MaximumNameLength = 200;

    private const int MaximumEmailLength = 256;

    private const int MaximumBlockReasonLength = 500;

    private const int MaximumSearchLength = 256;

    private readonly UserManager<ApplicationUser> _userManager;

    private readonly MawasemDbContext _dbContext;

    private readonly TimeProvider _timeProvider;

    public EmployeeManagementService(
        UserManager<ApplicationUser> userManager ,
        MawasemDbContext dbContext ,
        TimeProvider timeProvider )
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _timeProvider = timeProvider;
    }
}