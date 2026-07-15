using Mawasem.Application.Features.Employees.Contracts.Responses;
using Mawasem.Application.Features.Employees.Models;
using Mawasem.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Mawasem.Infrastructure.Employees;

public sealed partial class EmployeeManagementService
{
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
                    user => user.Id == actorUserId ,
                    cancellationToken);

        if ( actor is null || actor.IsBlocked )
        {
            return null;
        }

        var responses =
            await BuildEmployeeResponsesAsync(
                new[] { actor } ,
                cancellationToken);

        var actorResponse =
            responses.Single();

        if ( actorResponse.Roles.Count == 0 )
        {
            return null;
        }

        return new ActorAccess(
            actorResponse.Roles.Contains(
                SystemRoles.SuperAdmin ,
                StringComparer.OrdinalIgnoreCase) ,
            actorResponse.EffectivePermissions
                .ToHashSet(
                    StringComparer.OrdinalIgnoreCase));
    }

    private async Task<ManagedEmployee?>
        GetManagedEmployeeAsync(
            int employeeId ,
            CancellationToken cancellationToken )
    {
        if ( employeeId <= 0 )
        {
            return null;
        }

        var user =
            await _dbContext.Users
                .SingleOrDefaultAsync(
                    employee =>
                        employee.Id == employeeId ,
                    cancellationToken);

        if ( user is null )
        {
            return null;
        }

        var assignedRoles =
            await _userManager.GetRolesAsync(user);

        var dashboardRoles =
            assignedRoles
                .Where(SystemRoles.IsDashboardRole)
                .OrderBy(
                    roleName => roleName ,
                    StringComparer.Ordinal)
                .ToArray();

        if ( dashboardRoles.Length == 0 )
        {
            return null;
        }

        return new ManagedEmployee(
            user ,
            dashboardRoles);
    }

    private static IReadOnlyCollection<string>
        GetAssignableRoleNames(
            ActorAccess actorAccess )
    {
        return SystemRoles.DashboardRoles
            .Where(roleName =>
                !string.Equals(
                    roleName ,
                    SystemRoles.SuperAdmin ,
                    StringComparison.OrdinalIgnoreCase))
            .Where(roleName =>
                actorAccess.IsSuperAdmin ||
                !string.Equals(
                    roleName ,
                    SystemRoles.Admin ,
                    StringComparison.OrdinalIgnoreCase))
            .OrderBy(
                roleName => roleName ,
                StringComparer.Ordinal)
            .ToArray();
    }

    private async Task<IReadOnlyCollection<string>>
        GetAssignablePermissionNamesAsync(
            ActorAccess actorAccess ,
            CancellationToken cancellationToken )
    {
        var systemPermissionNames =
            SystemPermissions.All.ToArray();

        var activePermissionNames =
            await _dbContext.Permissions
                .AsNoTracking()
                .Where(permission =>
                    !permission.IsDeleted &&
                    permission.Name !=
                        SystemPermissions.Dashboard.Access &&
                    systemPermissionNames.Contains(
                        permission.Name))
                .Select(permission =>
                    permission.Name)
                .OrderBy(permissionName =>
                    permissionName)
                .ToArrayAsync(cancellationToken);

        if ( actorAccess.IsSuperAdmin )
        {
            return activePermissionNames;
        }

        return activePermissionNames
            .Where(
                actorAccess.EffectivePermissions.Contains)
            .ToArray();
    }

    private async Task<IReadOnlyCollection<EmployeeResponse>>
        BuildEmployeeResponsesAsync(
            IReadOnlyCollection<ApplicationUser> users ,
            CancellationToken cancellationToken )
    {
        if ( users.Count == 0 )
        {
            return Array.Empty<EmployeeResponse>();
        }

        var userIds =
            users
                .Select(user => user.Id)
                .ToArray();

        var dashboardRoleNames =
            SystemRoles.DashboardRoles.ToArray();

        var roleAssignments =
            await (
                from userRole
                    in _dbContext.UserRoles.AsNoTracking()
                join role
                    in _dbContext.Roles.AsNoTracking()
                    on userRole.RoleId equals role.Id
                where
                    userIds.Contains(userRole.UserId) &&
                    role.Name != null &&
                    dashboardRoleNames.Contains(role.Name)
                select new EmployeeRoleAssignment(
                    userRole.UserId ,
                    userRole.RoleId ,
                    role.Name!)
            ).ToListAsync(cancellationToken);

        var roleIds =
            roleAssignments
                .Select(assignment =>
                    assignment.RoleId)
                .Distinct()
                .ToArray();

        var rolePermissionAssignments =
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
            ).ToListAsync(cancellationToken);

        var directPermissionAssignments =
            await (
                from userPermission
                    in _dbContext.UserPermissions.AsNoTracking()
                join permission
                    in _dbContext.Permissions.AsNoTracking()
                    on userPermission.PermissionId
                    equals permission.Id
                where
                    userIds.Contains(
                        userPermission.UserId) &&
                    !permission.IsDeleted
                select new
                {
                    userPermission.UserId ,
                    PermissionName =
                        permission.Name
                }
            ).ToListAsync(cancellationToken);

        var rolesByUserId =
            roleAssignments
                .GroupBy(assignment =>
                    assignment.UserId)
                .ToDictionary(
                    group => group.Key ,
                    group => group.ToArray());

        var rolePermissionsByRoleId =
            rolePermissionAssignments
                .GroupBy(assignment =>
                    assignment.RoleId)
                .ToDictionary(
                    group => group.Key ,
                    group => group
                        .Select(assignment =>
                            assignment.PermissionName)
                        .ToArray());

        var directPermissionsByUserId =
            directPermissionAssignments
                .GroupBy(assignment =>
                    assignment.UserId)
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

        var responses =
            new List<EmployeeResponse>(
                users.Count);

        foreach ( var user in users )
        {
            if ( !rolesByUserId.TryGetValue(
                    user.Id ,
                    out var userRoleAssignments) )
            {
                userRoleAssignments =
                    Array.Empty<EmployeeRoleAssignment>();
            }

            if ( !directPermissionsByUserId.TryGetValue(
                    user.Id ,
                    out var directPermissions) )
            {
                directPermissions =
                    Array.Empty<string>();
            }

            var roleNames =
                userRoleAssignments
                    .Select(assignment =>
                        assignment.RoleName)
                    .Distinct(
                        StringComparer.OrdinalIgnoreCase)
                    .OrderBy(
                        roleName => roleName ,
                        StringComparer.Ordinal)
                    .ToArray();

            var effectivePermissions =
                new SortedSet<string>(
                    directPermissions ,
                    StringComparer.Ordinal);

            foreach ( var roleAssignment
                in userRoleAssignments )
            {
                if ( !rolePermissionsByRoleId.TryGetValue(
                        roleAssignment.RoleId ,
                        out var rolePermissions) )
                {
                    continue;
                }

                effectivePermissions.UnionWith(
                    rolePermissions);
            }

            responses.Add(
                new EmployeeResponse
                {
                    Id = user.Id ,
                    FullNameAr = user.FullNameAr ,
                    FullNameEn = user.FullNameEn ,
                    Email =
                        user.Email ?? string.Empty ,
                    IsBlocked = user.IsBlocked ,
                    BlockedAt = user.BlockedAt ,
                    BlockedReason =
                        user.BlockedReason ,
                    MustChangePassword =
                        user.MustChangePassword ,
                    Roles = roleNames ,
                    DirectPermissions =
                        directPermissions ,
                    EffectivePermissions =
                        effectivePermissions.ToArray()
                });
        }

        return responses;
    }

    private static bool TryValidateProfile(
        string? fullNameAr ,
        string? fullNameEn ,
        string? email ,
        out string normalizedFullNameAr ,
        out string normalizedFullNameEn ,
        out string normalizedEmail ,
        out string errorMessage )
    {
        normalizedFullNameAr =
            fullNameAr?.Trim() ?? string.Empty;

        normalizedFullNameEn =
            fullNameEn?.Trim() ?? string.Empty;

        normalizedEmail =
            email?.Trim() ?? string.Empty;

        if ( normalizedFullNameAr.Length == 0 )
        {
            errorMessage =
                "The Arabic employee name is required.";

            return false;
        }

        if ( normalizedFullNameAr.Length >
            MaximumNameLength )
        {
            errorMessage =
                $"The Arabic employee name cannot exceed {MaximumNameLength} characters.";

            return false;
        }

        if ( normalizedFullNameEn.Length == 0 )
        {
            errorMessage =
                "The English employee name is required.";

            return false;
        }

        if ( normalizedFullNameEn.Length >
            MaximumNameLength )
        {
            errorMessage =
                $"The English employee name cannot exceed {MaximumNameLength} characters.";

            return false;
        }

        if ( normalizedEmail.Length == 0 )
        {
            errorMessage =
                "The employee email is required.";

            return false;
        }

        if ( normalizedEmail.Length >
            MaximumEmailLength )
        {
            errorMessage =
                $"The employee email cannot exceed {MaximumEmailLength} characters.";

            return false;
        }

        if ( !new EmailAddressAttribute()
                .IsValid(normalizedEmail) )
        {
            errorMessage =
                "The employee email is not valid.";

            return false;
        }

        errorMessage = string.Empty;

        return true;
    }

    private static bool TryValidateTemporaryPassword(
        string? temporaryPassword ,
        string? confirmTemporaryPassword ,
        out string errorCode ,
        out string errorMessage )
    {
        if ( string.IsNullOrWhiteSpace(
                temporaryPassword) )
        {
            errorCode =
                EmployeeManagementErrorCodes.InvalidRequest;

            errorMessage =
                "The temporary password is required.";

            return false;
        }

        if ( string.IsNullOrWhiteSpace(
                confirmTemporaryPassword) )
        {
            errorCode =
                EmployeeManagementErrorCodes.InvalidRequest;

            errorMessage =
                "The temporary password confirmation is required.";

            return false;
        }

        if ( !string.Equals(
                temporaryPassword ,
                confirmTemporaryPassword ,
                StringComparison.Ordinal) )
        {
            errorCode =
                EmployeeManagementErrorCodes
                    .PasswordConfirmationMismatch;

            errorMessage =
                "The temporary password and confirmation do not match.";

            return false;
        }

        errorCode = string.Empty;
        errorMessage = string.Empty;

        return true;
    }

    private static bool TryValidateBlockReason(
        string? reason ,
        out string normalizedReason ,
        out string errorMessage )
    {
        normalizedReason =
            reason?.Trim() ?? string.Empty;

        if ( normalizedReason.Length == 0 )
        {
            errorMessage =
                "A block reason is required.";

            return false;
        }

        if ( normalizedReason.Length >
            MaximumBlockReasonLength )
        {
            errorMessage =
                $"The block reason cannot exceed {MaximumBlockReasonLength} characters.";

            return false;
        }

        errorMessage = string.Empty;

        return true;
    }

    private static bool TryResolveRequestedNames(
        IReadOnlyCollection<string>? requestedNames ,
        IReadOnlyCollection<string> allowedNames ,
        bool requireAtLeastOne ,
        out string[] resolvedNames ,
        out string errorMessage )
    {
        var allowedNamesByKey =
            allowedNames.ToDictionary(
                name => name ,
                name => name ,
                StringComparer.OrdinalIgnoreCase);

        var resolvedNameSet =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

        if ( requestedNames is not null )
        {
            foreach ( var requestedName
                in requestedNames )
            {
                var trimmedName =
                    requestedName?.Trim()
                    ?? string.Empty;

                if ( trimmedName.Length == 0 ||
                    !allowedNamesByKey.TryGetValue(
                        trimmedName ,
                        out var canonicalName) )
                {
                    resolvedNames =
                        Array.Empty<string>();

                    errorMessage =
                        $"'{trimmedName}' is not assignable.";

                    return false;
                }

                resolvedNameSet.Add(
                    canonicalName);
            }
        }

        if ( requireAtLeastOne &&
            resolvedNameSet.Count == 0 )
        {
            resolvedNames =
                Array.Empty<string>();

            errorMessage =
                "At least one dashboard role is required.";

            return false;
        }

        resolvedNames =
            resolvedNameSet
                .OrderBy(
                    name => name ,
                    StringComparer.Ordinal)
                .ToArray();

        errorMessage = string.Empty;

        return true;
    }

    private async Task RevokeAllActiveTokensAsync(
        int userId ,
        DateTime revokedAtUtc ,
        string? ipAddress ,
        string reason ,
        CancellationToken cancellationToken )
    {
        var activeTokens =
            await _dbContext.RefreshTokens
                .Where(token =>
                    token.UserId == userId &&
                    token.RevokedAtUtc == null &&
                    token.ExpiresAtUtc >
                        revokedAtUtc)
                .ToListAsync(cancellationToken);

        var normalizedIpAddress =
            NormalizeIpAddress(ipAddress);

        foreach ( var token in activeTokens )
        {
            token.RevokedAtUtc =
                revokedAtUtc;

            token.RevokedByIp =
                normalizedIpAddress;

            token.RevocationReason =
                reason;
        }
    }

    private static string JoinIdentityErrors(
        IEnumerable<IdentityError> errors )
    {
        return string.Join(
            "; " ,
            errors.Select(
                error => error.Description));
    }

    private static string? NormalizeIpAddress(
        string? ipAddress )
    {
        if ( string.IsNullOrWhiteSpace(ipAddress) )
        {
            return null;
        }

        var trimmedValue =
            ipAddress.Trim();

        return trimmedValue.Length <= 45
            ? trimmedValue
            : trimmedValue[..45];
    }

    private sealed record ActorAccess(
        bool IsSuperAdmin ,
        HashSet<string> EffectivePermissions );

    private sealed record ManagedEmployee(
        ApplicationUser User ,
        IReadOnlyCollection<string> Roles );

    private sealed record EmployeeRoleAssignment(
        int UserId ,
        int RoleId ,
        string RoleName );
}