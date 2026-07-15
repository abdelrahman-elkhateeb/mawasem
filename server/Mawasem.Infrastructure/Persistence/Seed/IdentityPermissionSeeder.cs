using Mawasem.Domain.Identity;
using Mawasem.Infrastructure.Persistence.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Mawasem.Infrastructure.Persistence.Seed;

public sealed class IdentityPermissionSeeder
{
    private const string SystemUserName = "System";

    private readonly MawasemDbContext _dbContext;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly TimeProvider _timeProvider;

    public IdentityPermissionSeeder(
        MawasemDbContext dbContext ,
        RoleManager<ApplicationRole> roleManager ,
        TimeProvider timeProvider )
    {
        _dbContext = dbContext;
        _roleManager = roleManager;
        _timeProvider = timeProvider;
    }

    public async Task SeedAsync(
        CancellationToken cancellationToken = default )
    {
        await SeedPermissionsAsync(cancellationToken);

        await AssignSuperAdminPermissionsAsync(
            cancellationToken);

        await RemoveCustomerPermissionsAsync(
            cancellationToken);

        // Save protected-role assignments before checking dashboard access.
        // This prevents Dashboard.Access from being added twice.
        await _dbContext.SaveChangesAsync(
            cancellationToken);

        await AssignDashboardAccessAsync(
            cancellationToken);

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private async Task SeedPermissionsAsync(
        CancellationToken cancellationToken )
    {
        var existingPermissions =
            await _dbContext.Permissions
                .IgnoreQueryFilters()
                .ToListAsync(cancellationToken);

        var permissionsByName =
            existingPermissions.ToDictionary(
                permission => permission.Name ,
                StringComparer.OrdinalIgnoreCase);

        var now =
            _timeProvider.GetUtcNow();

        foreach ( var permissionName in SystemPermissions.All )
        {
            if ( permissionsByName.TryGetValue(
                    permissionName ,
                    out var existingPermission) )
            {
                if ( existingPermission.IsDeleted )
                {
                    existingPermission.IsDeleted = false;
                    existingPermission.DeletedOn = null;
                    existingPermission.DeletedBy = null;
                    existingPermission.LastModifiedOn = now;
                    existingPermission.LastModifiedBy =
                        SystemUserName;
                }

                continue;
            }

            var permission =
                new Permission
                {
                    Name = permissionName ,
                    Description =
                        CreateDescription(permissionName) ,

                    CreatedOn = now ,
                    CreatedBy = SystemUserName ,

                    IsDeleted = false
                };

            _dbContext.Permissions.Add(permission);

            permissionsByName.Add(
                permissionName ,
                permission);
        }

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private async Task AssignSuperAdminPermissionsAsync(
        CancellationToken cancellationToken )
    {
        var superAdminRole =
            await _roleManager.FindByNameAsync(
                SystemRoles.SuperAdmin);

        if ( superAdminRole is null )
        {
            throw new InvalidOperationException(
                "The SuperAdmin role was not found. " +
                "Run the role seeder before the permission seeder.");
        }

        var permissionIds =
            await _dbContext.Permissions
                .Where(permission =>
                    !permission.IsDeleted &&
                    SystemPermissions.All.Contains(
                        permission.Name))
                .Select(permission => permission.Id)
                .ToListAsync(cancellationToken);

        var assignedPermissionIdList =
            await _dbContext.RolePermissions
                .Where(rolePermission =>
                    rolePermission.RoleId ==
                    superAdminRole.Id)
                .Select(rolePermission =>
                    rolePermission.PermissionId)
                .ToListAsync(cancellationToken);

        var assignedPermissionIds =
            assignedPermissionIdList.ToHashSet();

        foreach ( var permissionId in permissionIds )
        {
            if ( assignedPermissionIds.Contains(
                    permissionId) )
            {
                continue;
            }

            _dbContext.RolePermissions.Add(
                new RolePermission
                {
                    RoleId = superAdminRole.Id ,
                    PermissionId = permissionId
                });
        }
    }

    private async Task RemoveCustomerPermissionsAsync(
        CancellationToken cancellationToken )
    {
        var customerRole =
            await _roleManager.FindByNameAsync(
                SystemRoles.Customer);

        if ( customerRole is null )
        {
            throw new InvalidOperationException(
                "The Customer role was not found. " +
                "Run the role seeder before the permission seeder.");
        }

        var customerRolePermissions =
            await _dbContext.RolePermissions
                .Where(rolePermission =>
                    rolePermission.RoleId ==
                    customerRole.Id)
                .ToListAsync(cancellationToken);

        if ( customerRolePermissions.Count == 0 )
        {
            return;
        }

        _dbContext.RolePermissions.RemoveRange(
            customerRolePermissions);
    }

    private async Task AssignDashboardAccessAsync(
        CancellationToken cancellationToken )
    {
        var dashboardAccessPermission =
            await _dbContext.Permissions
                .SingleOrDefaultAsync(
                    permission =>
                        permission.Name ==
                        SystemPermissions.Dashboard.Access &&
                        !permission.IsDeleted ,
                    cancellationToken);

        if ( dashboardAccessPermission is null )
        {
            throw new InvalidOperationException(
                "The Dashboard.Access permission was not found.");
        }

        foreach ( var roleName in SystemRoles.DashboardRoles )
        {
            var role =
                await _roleManager.FindByNameAsync(roleName);

            if ( role is null )
            {
                throw new InvalidOperationException(
                    $"The dashboard role '{roleName}' was not found.");
            }

            var alreadyAssigned =
                await _dbContext.RolePermissions.AnyAsync(
                    rolePermission =>
                        rolePermission.RoleId == role.Id &&
                        rolePermission.PermissionId ==
                        dashboardAccessPermission.Id ,
                    cancellationToken);

            if ( alreadyAssigned )
            {
                continue;
            }

            _dbContext.RolePermissions.Add(
                new RolePermission
                {
                    RoleId = role.Id ,
                    PermissionId =
                        dashboardAccessPermission.Id
                });
        }
    }

    private static string CreateDescription(
        string permissionName )
    {
        return $"System permission: {permissionName}.";
    }
}