using Mawasem.Application.Features.Employees.Contracts.Requests;
using Mawasem.Application.Features.Employees.Contracts.Responses;
using Mawasem.Application.Features.Employees.Models;
using Mawasem.Domain.Identity;
using Microsoft.EntityFrameworkCore;

namespace Mawasem.Infrastructure.Employees;

public sealed partial class EmployeeManagementService
{
    public async Task<EmployeeManagementResult<EmployeeResponse>>
        AssignRolesAsync(
            int actorUserId ,
            int employeeId ,
            AssignEmployeeRolesRequest request ,
            CancellationToken cancellationToken = default )
    {
        ArgumentNullException.ThrowIfNull(request);

        var actorAccess =
            await GetActorAccessAsync(
                actorUserId ,
                cancellationToken);

        if ( actorAccess is null )
        {
            return EmployeeManagementResult<EmployeeResponse>
                .Failure(
                    EmployeeManagementErrorCodes.Forbidden ,
                    "The authenticated account cannot assign dashboard roles.");
        }

        var managedEmployee =
            await GetManagedEmployeeAsync(
                employeeId ,
                cancellationToken);

        if ( managedEmployee is null )
        {
            return EmployeeManagementResult<EmployeeResponse>
                .Failure(
                    EmployeeManagementErrorCodes.NotFound ,
                    "The dashboard employee was not found.");
        }

        if ( managedEmployee.Roles.Contains(
                SystemRoles.SuperAdmin ,
                StringComparer.OrdinalIgnoreCase) )
        {
            return EmployeeManagementResult<EmployeeResponse>
                .Failure(
                    EmployeeManagementErrorCodes
                        .CannotManageSuperAdmin ,
                    "The SuperAdmin roles cannot be changed through employee management.");
        }

        if ( actorUserId == employeeId )
        {
            return EmployeeManagementResult<EmployeeResponse>
                .Failure(
                    EmployeeManagementErrorCodes.CannotManageSelf ,
                    "You cannot change your own dashboard roles.");
        }

        if ( !actorAccess.IsSuperAdmin &&
            managedEmployee.Roles.Contains(
                SystemRoles.Admin ,
                StringComparer.OrdinalIgnoreCase) )
        {
            return EmployeeManagementResult<EmployeeResponse>
                .Failure(
                    EmployeeManagementErrorCodes.Forbidden ,
                    "Only a SuperAdmin can change an Admin account's roles.");
        }

        var assignableRoleNames =
            GetAssignableRoleNames(
                actorAccess);

        if ( !TryResolveRequestedNames(
                request.RoleNames ,
                assignableRoleNames ,
                requireAtLeastOne: true ,
                out var requestedRoleNames ,
                out var roleError) )
        {
            return EmployeeManagementResult<EmployeeResponse>
                .Failure(
                    EmployeeManagementErrorCodes.InvalidRole ,
                    roleError);
        }

        var rolesToRemove =
            managedEmployee.Roles
                .Except(
                    requestedRoleNames ,
                    StringComparer.OrdinalIgnoreCase)
                .ToArray();

        var rolesToAdd =
            requestedRoleNames
                .Except(
                    managedEmployee.Roles ,
                    StringComparer.OrdinalIgnoreCase)
                .ToArray();

        await using var transaction =
            await _dbContext.Database
                .BeginTransactionAsync(
                    cancellationToken);

        if ( rolesToRemove.Length > 0 )
        {
            var removeResult =
                await _userManager.RemoveFromRolesAsync(
                    managedEmployee.User ,
                    rolesToRemove);

            if ( !removeResult.Succeeded )
            {
                return EmployeeManagementResult<EmployeeResponse>
                    .Failure(
                        EmployeeManagementErrorCodes
                            .RoleAssignmentFailed ,
                        JoinIdentityErrors(
                            removeResult.Errors));
            }
        }

        if ( rolesToAdd.Length > 0 )
        {
            var addResult =
                await _userManager.AddToRolesAsync(
                    managedEmployee.User ,
                    rolesToAdd);

            if ( !addResult.Succeeded )
            {
                return EmployeeManagementResult<EmployeeResponse>
                    .Failure(
                        EmployeeManagementErrorCodes
                            .RoleAssignmentFailed ,
                        JoinIdentityErrors(
                            addResult.Errors));
            }
        }

        if ( rolesToRemove.Length > 0 ||
            rolesToAdd.Length > 0 )
        {
            var now =
                _timeProvider.GetUtcNow().UtcDateTime;

            await RevokeAllActiveTokensAsync(
                managedEmployee.User.Id ,
                now ,
                ipAddress: null ,
                "Dashboard roles changed." ,
                cancellationToken);

            await _dbContext.SaveChangesAsync(
                cancellationToken);
        }

        await transaction.CommitAsync(
            cancellationToken);

        var responses =
            await BuildEmployeeResponsesAsync(
                new[] { managedEmployee.User } ,
                cancellationToken);

        return EmployeeManagementResult<EmployeeResponse>
            .Success(
                responses.Single());
    }

    public async Task<EmployeeManagementResult<EmployeeResponse>>
        AssignPermissionsAsync(
            int actorUserId ,
            int employeeId ,
            AssignEmployeePermissionsRequest request ,
            CancellationToken cancellationToken = default )
    {
        ArgumentNullException.ThrowIfNull(request);

        var actorAccess =
            await GetActorAccessAsync(
                actorUserId ,
                cancellationToken);

        if ( actorAccess is null )
        {
            return EmployeeManagementResult<EmployeeResponse>
                .Failure(
                    EmployeeManagementErrorCodes.Forbidden ,
                    "The authenticated account cannot assign employee permissions.");
        }

        var managedEmployee =
            await GetManagedEmployeeAsync(
                employeeId ,
                cancellationToken);

        if ( managedEmployee is null )
        {
            return EmployeeManagementResult<EmployeeResponse>
                .Failure(
                    EmployeeManagementErrorCodes.NotFound ,
                    "The dashboard employee was not found.");
        }

        if ( managedEmployee.Roles.Contains(
                SystemRoles.SuperAdmin ,
                StringComparer.OrdinalIgnoreCase) )
        {
            return EmployeeManagementResult<EmployeeResponse>
                .Failure(
                    EmployeeManagementErrorCodes
                        .CannotManageSuperAdmin ,
                    "The SuperAdmin permissions cannot be changed through employee management.");
        }

        if ( actorUserId == employeeId )
        {
            return EmployeeManagementResult<EmployeeResponse>
                .Failure(
                    EmployeeManagementErrorCodes.CannotManageSelf ,
                    "You cannot change your own direct permissions.");
        }

        if ( !actorAccess.IsSuperAdmin &&
            managedEmployee.Roles.Contains(
                SystemRoles.Admin ,
                StringComparer.OrdinalIgnoreCase) )
        {
            return EmployeeManagementResult<EmployeeResponse>
                .Failure(
                    EmployeeManagementErrorCodes.Forbidden ,
                    "Only a SuperAdmin can change an Admin account's permissions.");
        }

        var assignablePermissionNames =
            await GetAssignablePermissionNamesAsync(
                actorAccess ,
                cancellationToken);

        if ( !TryResolveRequestedNames(
                request.PermissionNames ,
                assignablePermissionNames ,
                requireAtLeastOne: false ,
                out var requestedPermissionNames ,
                out var permissionError) )
        {
            return EmployeeManagementResult<EmployeeResponse>
                .Failure(
                    EmployeeManagementErrorCodes.InvalidPermission ,
                    permissionError);
        }

        var requestedPermissions =
            await _dbContext.Permissions
                .AsNoTracking()
                .Where(permission =>
                    !permission.IsDeleted &&
                    requestedPermissionNames.Contains(
                        permission.Name))
                .Select(permission =>
                    new
                    {
                        permission.Id ,
                        permission.Name
                    })
                .ToArrayAsync(cancellationToken);

        if ( requestedPermissions.Length !=
            requestedPermissionNames.Length )
        {
            return EmployeeManagementResult<EmployeeResponse>
                .Failure(
                    EmployeeManagementErrorCodes.InvalidPermission ,
                    "One or more selected permissions are unavailable.");
        }

        var assignablePermissionIds =
            await _dbContext.Permissions
                .AsNoTracking()
                .Where(permission =>
                    !permission.IsDeleted &&
                    assignablePermissionNames.Contains(
                        permission.Name))
                .Select(permission =>
                    permission.Id)
                .ToArrayAsync(cancellationToken);

        var requestedPermissionIds =
            requestedPermissions
                .Select(permission =>
                    permission.Id)
                .ToHashSet();

        var assignablePermissionIdSet =
            assignablePermissionIds
                .ToHashSet();

        var currentAssignments =
            await _dbContext.UserPermissions
                .Where(userPermission =>
                    userPermission.UserId ==
                        employeeId)
                .ToListAsync(cancellationToken);

        var assignmentsToRemove =
            currentAssignments
                .Where(assignment =>
                    assignablePermissionIdSet.Contains(
                        assignment.PermissionId) &&
                    !requestedPermissionIds.Contains(
                        assignment.PermissionId))
                .ToArray();

        _dbContext.UserPermissions.RemoveRange(
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

            _dbContext.UserPermissions.Add(
                new UserPermission
                {
                    UserId = employeeId ,
                    PermissionId = permissionId
                });
        }

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        var responses =
            await BuildEmployeeResponsesAsync(
                new[] { managedEmployee.User } ,
                cancellationToken);

        return EmployeeManagementResult<EmployeeResponse>
            .Success(
                responses.Single());
    }
}