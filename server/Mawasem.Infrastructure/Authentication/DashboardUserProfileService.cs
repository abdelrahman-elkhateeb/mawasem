using Mawasem.Application.Features.Authentication.Contracts.Responses;
using Mawasem.Application.Features.Authentication.Interfaces;
using Mawasem.Application.Features.Authentication.Models;
using Mawasem.Domain.Identity;
using Mawasem.Infrastructure.Persistence.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Mawasem.Infrastructure.Authentication;

public sealed class DashboardUserProfileService
    : IDashboardUserProfileService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly MawasemDbContext _dbContext;

    public DashboardUserProfileService(
        UserManager<ApplicationUser> userManager ,
        MawasemDbContext dbContext )
    {
        _userManager = userManager;
        _dbContext = dbContext;
    }

    public async Task<DashboardUserProfileResult> GetAsync(
        int userId ,
        CancellationToken cancellationToken = default )
    {
        if ( userId <= 0 )
        {
            return DashboardUserProfileResult.Failure(
                AuthenticationErrorCodes.InvalidCredentials ,
                "The authenticated dashboard account is invalid.");
        }

        var user =
            await _dbContext.Users
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    applicationUser =>
                        applicationUser.Id == userId ,
                    cancellationToken);

        if ( user is null )
        {
            return DashboardUserProfileResult.Failure(
                AuthenticationErrorCodes.InvalidCredentials ,
                "The authenticated dashboard account was not found.");
        }

        if ( user.IsBlocked )
        {
            return DashboardUserProfileResult.Failure(
                AuthenticationErrorCodes.AccountBlocked ,
                "This dashboard account has been blocked.");
        }

        var assignedRoles =
            await _userManager.GetRolesAsync(user);

        var dashboardRoles =
            assignedRoles
                .Where(SystemRoles.IsDashboardRole)
                .OrderBy(roleName => roleName)
                .ToArray();

        if ( dashboardRoles.Length == 0 )
        {
            return DashboardUserProfileResult.Failure(
                AuthenticationErrorCodes.InvalidCredentials ,
                "The authenticated account cannot access the dashboard.");
        }

        var roleIds =
            await _dbContext.Roles
                .AsNoTracking()
                .Where(role =>
                    role.Name != null &&
                    dashboardRoles.Contains(role.Name))
                .Select(role => role.Id)
                .ToListAsync(cancellationToken);

        var permissions =
            await (
                from rolePermission
                    in _dbContext.RolePermissions.AsNoTracking()
                join permission
                    in _dbContext.Permissions.AsNoTracking()
                    on rolePermission.PermissionId
                    equals permission.Id
                where
                    roleIds.Contains(rolePermission.RoleId) &&
                    !permission.IsDeleted
                select permission.Name
            )
            .Distinct()
            .OrderBy(permissionName => permissionName)
            .ToArrayAsync(cancellationToken);

        var response =
            new DashboardAuthenticatedUserResponse
            {
                Id = user.Id ,
                FullNameAr = user.FullNameAr ,
                FullNameEn = user.FullNameEn ,
                Email = user.Email ?? string.Empty ,
                MustChangePassword =
                    user.MustChangePassword ,
                Roles = dashboardRoles ,
                Permissions = permissions
            };

        return DashboardUserProfileResult.Success(response);
    }
}