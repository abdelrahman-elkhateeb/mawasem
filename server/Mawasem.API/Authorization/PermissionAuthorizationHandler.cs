using Mawasem.Infrastructure.Persistence.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;

namespace Mawasem.API.Authorization;

public sealed class PermissionAuthorizationHandler
    : AuthorizationHandler<PermissionAuthorizationRequirement>
{
    private readonly MawasemDbContext _dbContext;

    public PermissionAuthorizationHandler(
        MawasemDbContext dbContext )
    {
        _dbContext = dbContext;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context ,
        PermissionAuthorizationRequirement requirement )
    {
        if ( context.User.Identity?.IsAuthenticated != true )
        {
            return;
        }

        var userIdValue =
            context.User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

        if ( !int.TryParse(
                userIdValue ,
                NumberStyles.None ,
                CultureInfo.InvariantCulture ,
                out var userId) )
        {
            return;
        }

        var rolePermissionIds =
            from user in _dbContext.Users
            join userRole in _dbContext.UserRoles
                on user.Id equals userRole.UserId
            join rolePermission in _dbContext.RolePermissions
                on userRole.RoleId equals rolePermission.RoleId
            join permission in _dbContext.Permissions
                on rolePermission.PermissionId equals permission.Id
            where
                user.Id == userId
                && !user.IsBlocked
                && !user.MustChangePassword
                && !permission.IsDeleted
                && permission.Name == requirement.Permission
            select permission.Id;

        var userPermissionIds =
            from user in _dbContext.Users
            join userPermission in _dbContext.UserPermissions
                on user.Id equals userPermission.UserId
            join permission in _dbContext.Permissions
                on userPermission.PermissionId equals permission.Id
            where
                user.Id == userId
                && !user.IsBlocked
                && !user.MustChangePassword
                && !permission.IsDeleted
                && permission.Name == requirement.Permission
            select permission.Id;

        var hasPermission =
            await rolePermissionIds
                .Concat(userPermissionIds)
                .AnyAsync();

        if ( hasPermission )
        {
            context.Succeed(requirement);
        }
    }
}