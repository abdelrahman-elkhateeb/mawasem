using Mawasem.Application.Features.Roles.Contracts.Requests;
using Mawasem.Application.Features.Roles.Contracts.Responses;
using Mawasem.Application.Features.Roles.Interfaces;
using Mawasem.Application.Features.Roles.Models;
using Mawasem.Domain.Identity;
using Mawasem.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Mawasem.Infrastructure.Roles;

public sealed class RolePermissionManagementService
    : IRolePermissionManagementService
{
    private readonly MawasemDbContext _dbContext;

    public RolePermissionManagementService(
        MawasemDbContext dbContext )
    {
        _dbContext = dbContext;
    }

    public async Task<RoleManagementResult<RoleListResponse>>
        GetListAsync(
            int actorUserId ,
            CancellationToken cancellationToken = default )
    {
        var actorAccess =
            await GetActorAccessAsync(
                actorUserId ,
                cancellationToken);

        if ( actorAccess is null )
        {
            return RoleManagementResult<RoleListResponse>
                .Failure(
                    RoleManagementErrorCodes.Forbidden ,
                    "The authenticated account cannot view dashboard roles.");
        }

        var roleResponses =
            await BuildRoleResponsesAsync(
                actorAccess ,
                cancellationToken);

        return RoleManagementResult<RoleListResponse>
            .Success(
                new RoleListResponse
                {
                    Items = roleResponses
                });
    }

    public async Task<RoleManagementResult<RoleResponse>>
        GetByNameAsync(
            int actorUserId ,
            string roleName ,
            CancellationToken cancellationToken = default )
    {
        var canonicalRoleName =
            ResolveSystemRoleName(roleName);

        if ( canonicalRoleName is null )
        {
            return RoleManagementResult<RoleResponse>
                .Failure(
                    RoleManagementErrorCodes.NotFound ,
                    "The system role was not found.");
        }

        var listResult =
            await GetListAsync(
                actorUserId ,
                cancellationToken);

        if ( !listResult.Succeeded ||
            listResult.Response is null )
        {
            return RoleManagementResult<RoleResponse>
                .Failure(
                    listResult.ErrorCode
                    ?? RoleManagementErrorCodes.Forbidden ,
                    listResult.ErrorMessage
                    ?? "The system role could not be retrieved.");
        }

        var role =
            listResult.Response.Items
                .SingleOrDefault(item =>
                    string.Equals(
                        item.Name ,
                        canonicalRoleName ,
                        StringComparison.OrdinalIgnoreCase));

        if ( role is null )
        {
            return RoleManagementResult<RoleResponse>
                .Failure(
                    RoleManagementErrorCodes.NotFound ,
                    "The system role was not found.");
        }

        return RoleManagementResult<RoleResponse>
            .Success(role);
    }

    public async Task<
        RoleManagementResult<RolePermissionOptionsResponse>>
        GetPermissionOptionsAsync(
            int actorUserId ,
            CancellationToken cancellationToken = default )
    {
        var actorAccess =
            await GetActorAccessAsync(
                actorUserId ,
                cancellationToken);

        if ( actorAccess is null )
        {
            return RoleManagementResult<
                RolePermissionOptionsResponse>.Failure(
                    RoleManagementErrorCodes.Forbidden ,
                    "The authenticated account cannot view role-permission options.");
        }

        var assignablePermissions =
            await GetAssignablePermissionsAsync(
                actorAccess ,
                cancellationToken);

        var options =
            assignablePermissions
                .OrderBy(permission =>
                    permission.Name)
                .Select(permission =>
                    new PermissionOptionResponse
                    {
                        Name = permission.Name ,
                        Description =
                            permission.Description ,
                        IsRequired =
                            string.Equals(
                                permission.Name ,
                                SystemPermissions
                                    .Dashboard.Access ,
                                StringComparison.Ordinal)
                    })
                .ToArray();

        return RoleManagementResult<
            RolePermissionOptionsResponse>.Success(
                new RolePermissionOptionsResponse
                {
                    Items = options
                });
    }

    public async Task<RoleManagementResult<RoleResponse>>
        UpdatePermissionsAsync(
            int actorUserId ,
            string roleName ,
            UpdateRolePermissionsRequest request ,
            CancellationToken cancellationToken = default )
    {
        ArgumentNullException.ThrowIfNull(request);

        var actorAccess =
            await GetActorAccessAsync(
                actorUserId ,
                cancellationToken);

        if ( actorAccess is null )
        {
            return RoleManagementResult<RoleResponse>
                .Failure(
                    RoleManagementErrorCodes.Forbidden ,
                    "The authenticated account cannot manage role permissions.");
        }

        var canonicalRoleName =
            ResolveSystemRoleName(roleName);

        if ( canonicalRoleName is null )
        {
            return RoleManagementResult<RoleResponse>
                .Failure(
                    RoleManagementErrorCodes.NotFound ,
                    "The system role was not found.");
        }

        if ( IsProtectedRole(canonicalRoleName) )
        {
            return RoleManagementResult<RoleResponse>
                .Failure(
                    RoleManagementErrorCodes.ProtectedRole ,
                    $"The {canonicalRoleName} role is protected and cannot be modified.");
        }

        if ( !CanManageRole(
                actorAccess ,
                canonicalRoleName) )
        {
            return RoleManagementResult<RoleResponse>
                .Failure(
                    RoleManagementErrorCodes.Forbidden ,
                    "The authenticated account cannot modify this role.");
        }

        var role =
            await _dbContext.Roles
                .SingleOrDefaultAsync(
                    applicationRole =>
                        applicationRole.Name ==
                        canonicalRoleName ,
                    cancellationToken);

        if ( role is null )
        {
            return RoleManagementResult<RoleResponse>
                .Failure(
                    RoleManagementErrorCodes.NotFound ,
                    "The system role was not found.");
        }

        var assignablePermissions =
            await GetAssignablePermissionsAsync(
                actorAccess ,
                cancellationToken);

        var permissionsByName =
            assignablePermissions.ToDictionary(
                permission => permission.Name ,
                StringComparer.OrdinalIgnoreCase);

        var requestedPermissionNames =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

        if ( request.PermissionNames is not null )
        {
            foreach ( var requestedPermissionName
                in request.PermissionNames )
            {
                var trimmedName =
                    requestedPermissionName?.Trim()
                    ?? string.Empty;

                if ( trimmedName.Length == 0 ||
                    !permissionsByName.TryGetValue(
                        trimmedName ,
                        out var permission) )
                {
                    return RoleManagementResult<RoleResponse>
                        .Failure(
                            RoleManagementErrorCodes
                                .InvalidPermission ,
                            $"'{trimmedName}' is not assignable.");
                }

                requestedPermissionNames.Add(
                    permission.Name);
            }
        }

        if ( !permissionsByName.TryGetValue(
                SystemPermissions.Dashboard.Access ,
                out var dashboardAccessPermission) )
        {
            return RoleManagementResult<RoleResponse>
                .Failure(
                    RoleManagementErrorCodes.Forbidden ,
                    "The authenticated account cannot preserve mandatory dashboard access.");
        }

        requestedPermissionNames.Add(
            dashboardAccessPermission.Name);

        var requestedPermissionIds =
            requestedPermissionNames
                .Select(permissionName =>
                    permissionsByName[permissionName].Id)
                .ToHashSet();

        var assignablePermissionIds =
            assignablePermissions
                .Select(permission =>
                    permission.Id)
                .ToHashSet();

        var currentAssignments =
            await _dbContext.RolePermissions
                .Where(rolePermission =>
                    rolePermission.RoleId == role.Id)
                .ToListAsync(cancellationToken);

        var assignmentsToRemove =
            currentAssignments
                .Where(assignment =>
                    assignablePermissionIds.Contains(
                        assignment.PermissionId) &&
                    !requestedPermissionIds.Contains(
                        assignment.PermissionId))
                .ToArray();

        _dbContext.RolePermissions.RemoveRange(
            assignmentsToRemove);

        var currentPermissionIds =
            currentAssignments
                .Select(assignment =>
                    assignment.PermissionId)
                .ToHashSet();

        foreach ( var permissionId
            in requestedPermissionIds )
        {
            if ( currentPermissionIds.Contains(
                    permissionId) )
            {
                continue;
            }

            _dbContext.RolePermissions.Add(
                new RolePermission
                {
                    RoleId = role.Id ,
                    PermissionId = permissionId
                });
        }

        try
        {
            await _dbContext.SaveChangesAsync(
                cancellationToken);
        }
        catch ( DbUpdateException )
        {
            return RoleManagementResult<RoleResponse>
                .Failure(
                    RoleManagementErrorCodes.UpdateFailed ,
                    "The role permissions could not be updated.");
        }

        return await GetByNameAsync(
            actorUserId ,
            canonicalRoleName ,
            cancellationToken);
    }

    private async Task<ActorAccess?> GetActorAccessAsync(
        int actorUserId ,
        CancellationToken cancellationToken )
    {
        if ( actorUserId <= 0 )
        {
            return null;
        }

        var actor =
            await _dbContext.Users
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    user =>
                        user.Id == actorUserId ,
                    cancellationToken);

        if ( actor is null ||
            actor.IsBlocked ||
            actor.MustChangePassword )
        {
            return null;
        }

        var dashboardRoleNames =
            SystemRoles.DashboardRoles.ToArray();

        var actorRoles =
            await (
                from userRole
                    in _dbContext.UserRoles.AsNoTracking()
                join role
                    in _dbContext.Roles.AsNoTracking()
                    on userRole.RoleId equals role.Id
                where
                    userRole.UserId == actorUserId &&
                    role.Name != null &&
                    dashboardRoleNames.Contains(role.Name)
                select new
                {
                    role.Id ,
                    Name = role.Name
                }
            ).ToArrayAsync(cancellationToken);

        if ( actorRoles.Length == 0 )
        {
            return null;
        }

        var actorRoleIds =
            actorRoles
                .Select(role => role.Id)
                .ToArray();

        var rolePermissionNames =
            from rolePermission
                in _dbContext.RolePermissions.AsNoTracking()
            join permission
                in _dbContext.Permissions.AsNoTracking()
                on rolePermission.PermissionId
                equals permission.Id
            where
                actorRoleIds.Contains(
                    rolePermission.RoleId) &&
                !permission.IsDeleted
            select permission.Name;

        var userPermissionNames =
            from userPermission
                in _dbContext.UserPermissions.AsNoTracking()
            join permission
                in _dbContext.Permissions.AsNoTracking()
                on userPermission.PermissionId
                equals permission.Id
            where
                userPermission.UserId == actorUserId &&
                !permission.IsDeleted
            select permission.Name;

        var effectivePermissions =
            await rolePermissionNames
                .Concat(userPermissionNames)
                .Distinct()
                .ToArrayAsync(cancellationToken);

        var isSuperAdmin =
            actorRoles.Any(role =>
                string.Equals(
                    role.Name ,
                    SystemRoles.SuperAdmin ,
                    StringComparison.OrdinalIgnoreCase));

        return new ActorAccess(
            isSuperAdmin ,
            effectivePermissions.ToHashSet(
                StringComparer.OrdinalIgnoreCase));
    }

    private async Task<IReadOnlyCollection<Permission>>
        GetAssignablePermissionsAsync(
            ActorAccess actorAccess ,
            CancellationToken cancellationToken )
    {
        var systemPermissionNames =
            SystemPermissions.All.ToArray();

        var activePermissions =
            await _dbContext.Permissions
                .AsNoTracking()
                .Where(permission =>
                    !permission.IsDeleted &&
                    systemPermissionNames.Contains(
                        permission.Name))
                .OrderBy(permission =>
                    permission.Name)
                .ToArrayAsync(cancellationToken);

        if ( actorAccess.IsSuperAdmin )
        {
            return activePermissions;
        }

        return activePermissions
            .Where(permission =>
                actorAccess.EffectivePermissions.Contains(
                    permission.Name))
            .ToArray();
    }

    private async Task<IReadOnlyCollection<RoleResponse>>
        BuildRoleResponsesAsync(
            ActorAccess actorAccess ,
            CancellationToken cancellationToken )
    {
        var systemRoleNames =
            SystemRoles.All.ToArray();

        var roles =
            await _dbContext.Roles
                .AsNoTracking()
                .Where(role =>
                    role.Name != null &&
                    systemRoleNames.Contains(role.Name))
                .ToArrayAsync(cancellationToken);

        var roleIds =
            roles
                .Select(role => role.Id)
                .ToArray();

        var userCounts =
            await _dbContext.UserRoles
                .AsNoTracking()
                .Where(userRole =>
                    roleIds.Contains(userRole.RoleId))
                .GroupBy(userRole =>
                    userRole.RoleId)
                .Select(group =>
                    new
                    {
                        RoleId = group.Key ,
                        Count = group.Count()
                    })
                .ToDictionaryAsync(
                    item => item.RoleId ,
                    item => item.Count ,
                    cancellationToken);

        var permissionAssignments =
            await (
                from rolePermission
                    in _dbContext.RolePermissions.AsNoTracking()
                join permission
                    in _dbContext.Permissions.AsNoTracking()
                    on rolePermission.PermissionId
                    equals permission.Id
                where
                    roleIds.Contains(
                        rolePermission.RoleId) &&
                    !permission.IsDeleted
                select new
                {
                    rolePermission.RoleId ,
                    PermissionName =
                        permission.Name
                }
            ).ToArrayAsync(cancellationToken);

        var permissionsByRoleId =
            permissionAssignments
                .GroupBy(assignment =>
                    assignment.RoleId)
                .ToDictionary(
                    group => group.Key ,
                    group => group
                        .Select(assignment =>
                            assignment.PermissionName)
                        .Distinct(
                            StringComparer.Ordinal)
                        .OrderBy(
                            permissionName =>
                                permissionName ,
                            StringComparer.Ordinal)
                        .ToArray());

        var rolesByName =
            roles
                .Where(role =>
                    role.Name is not null)
                .ToDictionary(
                    role => role.Name! ,
                    StringComparer.OrdinalIgnoreCase);

        var orderedRoleNames =
            new[]
            {
                SystemRoles.SuperAdmin ,
                SystemRoles.Admin ,
                SystemRoles.SalesEmployee ,
                SystemRoles.DeliveryEmployee ,
                SystemRoles.SupportEmployee ,
                SystemRoles.StoreEmployee ,
                SystemRoles.Customer
            };

        var responses =
            new List<RoleResponse>(
                orderedRoleNames.Length);

        foreach ( var roleName in orderedRoleNames )
        {
            if ( !rolesByName.TryGetValue(
                    roleName ,
                    out var role) )
            {
                continue;
            }

            userCounts.TryGetValue(
                role.Id ,
                out var assignedUserCount);

            if ( !permissionsByRoleId.TryGetValue(
                    role.Id ,
                    out var permissionNames) )
            {
                permissionNames =
                    Array.Empty<string>();
            }

            responses.Add(
                new RoleResponse
                {
                    Name = roleName ,
                    IsProtected =
                        IsProtectedRole(roleName) ,
                    CanManagePermissions =
                        CanManageRole(
                            actorAccess ,
                            roleName) ,
                    AssignedUserCount =
                        assignedUserCount ,
                    PermissionNames =
                        permissionNames
                });
        }

        return responses;
    }

    private static bool CanManageRole(
        ActorAccess actorAccess ,
        string roleName )
    {
        if ( IsProtectedRole(roleName) )
        {
            return false;
        }

        if ( actorAccess.IsSuperAdmin )
        {
            return true;
        }

        return !string.Equals(
            roleName ,
            SystemRoles.Admin ,
            StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsProtectedRole(
        string roleName )
    {
        return string.Equals(
                roleName ,
                SystemRoles.SuperAdmin ,
                StringComparison.OrdinalIgnoreCase)
            ||
            string.Equals(
                roleName ,
                SystemRoles.Customer ,
                StringComparison.OrdinalIgnoreCase);
    }

    private static string? ResolveSystemRoleName(
        string? roleName )
    {
        if ( string.IsNullOrWhiteSpace(roleName) )
        {
            return null;
        }

        var trimmedRoleName =
            roleName.Trim();

        return SystemRoles.All
            .SingleOrDefault(systemRoleName =>
                string.Equals(
                    systemRoleName ,
                    trimmedRoleName ,
                    StringComparison.OrdinalIgnoreCase));
    }

    private sealed record ActorAccess(
        bool IsSuperAdmin ,
        HashSet<string> EffectivePermissions );
}