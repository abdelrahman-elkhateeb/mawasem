using Mawasem.Application.Features.Employees.Contracts.Requests;
using Mawasem.Application.Features.Employees.Contracts.Responses;
using Mawasem.Application.Features.Employees.Models;
using Mawasem.Domain.Identity;
using Microsoft.EntityFrameworkCore;

namespace Mawasem.Infrastructure.Employees;

public sealed partial class EmployeeManagementService
{
    public async Task<EmployeeManagementResult<EmployeeResponse>>
        CreateAsync(
            int actorUserId ,
            CreateEmployeeRequest request ,
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
                    "The authenticated account cannot create dashboard employees.");
        }

        if ( !TryValidateProfile(
                request.FullNameAr ,
                request.FullNameEn ,
                request.Email ,
                out var fullNameAr ,
                out var fullNameEn ,
                out var email ,
                out var profileError) )
        {
            return EmployeeManagementResult<EmployeeResponse>
                .Failure(
                    EmployeeManagementErrorCodes.InvalidRequest ,
                    profileError);
        }

        if ( !TryValidateTemporaryPassword(
                request.TemporaryPassword ,
                request.ConfirmTemporaryPassword ,
                out var passwordErrorCode ,
                out var passwordError) )
        {
            return EmployeeManagementResult<EmployeeResponse>
                .Failure(
                    passwordErrorCode ,
                    passwordError);
        }

        var assignableRoleNames =
            GetAssignableRoleNames(
                actorAccess);

        if ( !TryResolveRequestedNames(
                request.RoleNames ,
                assignableRoleNames ,
                requireAtLeastOne: true ,
                out var roleNames ,
                out var roleError) )
        {
            return EmployeeManagementResult<EmployeeResponse>
                .Failure(
                    EmployeeManagementErrorCodes.InvalidRole ,
                    roleError);
        }

        var assignablePermissionNames =
            await GetAssignablePermissionNamesAsync(
                actorAccess ,
                cancellationToken);

        if ( !TryResolveRequestedNames(
                request.PermissionNames ,
                assignablePermissionNames ,
                requireAtLeastOne: false ,
                out var permissionNames ,
                out var permissionError) )
        {
            return EmployeeManagementResult<EmployeeResponse>
                .Failure(
                    EmployeeManagementErrorCodes.InvalidPermission ,
                    permissionError);
        }

        var normalizedEmail =
            _userManager.NormalizeEmail(email);

        if ( string.IsNullOrWhiteSpace(
                normalizedEmail) )
        {
            return EmployeeManagementResult<EmployeeResponse>
                .Failure(
                    EmployeeManagementErrorCodes.InvalidRequest ,
                    "The employee email could not be normalized.");
        }

        var emailAlreadyExists =
            await _dbContext.Users
                .AsNoTracking()
                .AnyAsync(
                    user =>
                        user.NormalizedEmail ==
                        normalizedEmail ,
                    cancellationToken);

        if ( emailAlreadyExists )
        {
            return EmployeeManagementResult<EmployeeResponse>
                .Failure(
                    EmployeeManagementErrorCodes
                        .EmailAlreadyRegistered ,
                    "An account with this email already exists.");
        }

        var permissionIds =
            await _dbContext.Permissions
                .AsNoTracking()
                .Where(permission =>
                    !permission.IsDeleted &&
                    permissionNames.Contains(
                        permission.Name))
                .Select(permission =>
                    permission.Id)
                .ToArrayAsync(cancellationToken);

        if ( permissionIds.Length !=
            permissionNames.Length )
        {
            return EmployeeManagementResult<EmployeeResponse>
                .Failure(
                    EmployeeManagementErrorCodes.InvalidPermission ,
                    "One or more selected permissions are unavailable.");
        }

        var employee =
            new ApplicationUser
            {
                UserName = email ,
                Email = email ,
                EmailConfirmed = true ,

                FullNameAr = fullNameAr ,
                FullNameEn = fullNameEn ,

                PhoneNumber = null ,
                PhoneNumberConfirmed = false ,

                BirthDate = null ,
                Gender = null ,
                ReferralSource = null ,

                IsBlocked = false ,
                BlockedAt = null ,
                BlockedReason = null ,

                MustChangePassword = true ,
                LockoutEnabled = true
            };

        await using var transaction =
            await _dbContext.Database
                .BeginTransactionAsync(
                    cancellationToken);

        var createResult =
            await _userManager.CreateAsync(
                employee ,
                request.TemporaryPassword);

        if ( !createResult.Succeeded )
        {
            return EmployeeManagementResult<EmployeeResponse>
                .Failure(
                    EmployeeManagementErrorCodes.CreationFailed ,
                    JoinIdentityErrors(
                        createResult.Errors));
        }

        var roleResult =
            await _userManager.AddToRolesAsync(
                employee ,
                roleNames);

        if ( !roleResult.Succeeded )
        {
            return EmployeeManagementResult<EmployeeResponse>
                .Failure(
                    EmployeeManagementErrorCodes
                        .RoleAssignmentFailed ,
                    JoinIdentityErrors(
                        roleResult.Errors));
        }

        foreach ( var permissionId
            in permissionIds )
        {
            _dbContext.UserPermissions.Add(
                new UserPermission
                {
                    UserId = employee.Id ,
                    PermissionId = permissionId
                });
        }

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        await transaction.CommitAsync(
            cancellationToken);

        var responses =
            await BuildEmployeeResponsesAsync(
                new[] { employee } ,
                cancellationToken);

        return EmployeeManagementResult<EmployeeResponse>
            .Success(
                responses.Single());
    }

    public async Task<EmployeeManagementResult<EmployeeResponse>>
        UpdateAsync(
            int actorUserId ,
            int employeeId ,
            UpdateEmployeeRequest request ,
            string? ipAddress ,
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
                    "The authenticated account cannot update dashboard employees.");
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
                    "The SuperAdmin account cannot be modified through employee management.");
        }

        if ( !actorAccess.IsSuperAdmin &&
            managedEmployee.Roles.Contains(
                SystemRoles.Admin ,
                StringComparer.OrdinalIgnoreCase) )
        {
            return EmployeeManagementResult<EmployeeResponse>
                .Failure(
                    EmployeeManagementErrorCodes.Forbidden ,
                    "Only a SuperAdmin can modify an Admin account.");
        }

        if ( !TryValidateProfile(
                request.FullNameAr ,
                request.FullNameEn ,
                request.Email ,
                out var fullNameAr ,
                out var fullNameEn ,
                out var email ,
                out var profileError) )
        {
            return EmployeeManagementResult<EmployeeResponse>
                .Failure(
                    EmployeeManagementErrorCodes.InvalidRequest ,
                    profileError);
        }

        var normalizedEmail =
            _userManager.NormalizeEmail(email);

        if ( string.IsNullOrWhiteSpace(
                normalizedEmail) )
        {
            return EmployeeManagementResult<EmployeeResponse>
                .Failure(
                    EmployeeManagementErrorCodes.InvalidRequest ,
                    "The employee email could not be normalized.");
        }

        var emailAlreadyExists =
            await _dbContext.Users
                .AsNoTracking()
                .AnyAsync(
                    user =>
                        user.Id != employeeId &&
                        user.NormalizedEmail ==
                            normalizedEmail ,
                    cancellationToken);

        if ( emailAlreadyExists )
        {
            return EmployeeManagementResult<EmployeeResponse>
                .Failure(
                    EmployeeManagementErrorCodes
                        .EmailAlreadyRegistered ,
                    "An account with this email already exists.");
        }

        await using var transaction =
            await _dbContext.Database
                .BeginTransactionAsync(
                    cancellationToken);

        var employee =
            managedEmployee.User;

        employee.FullNameAr = fullNameAr;
        employee.FullNameEn = fullNameEn;
        employee.Email = email;
        employee.UserName = email;
        employee.EmailConfirmed = true;

        var updateResult =
            await _userManager.UpdateAsync(
                employee);

        if ( !updateResult.Succeeded )
        {
            return EmployeeManagementResult<EmployeeResponse>
                .Failure(
                    EmployeeManagementErrorCodes.UpdateFailed ,
                    JoinIdentityErrors(
                        updateResult.Errors));
        }

        var now =
            _timeProvider.GetUtcNow().UtcDateTime;

        await RevokeAllActiveTokensAsync(
            employee.Id ,
            now ,
            ipAddress ,
            "Dashboard account profile updated." ,
            cancellationToken);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        await transaction.CommitAsync(
            cancellationToken);

        var responses =
            await BuildEmployeeResponsesAsync(
                new[] { employee } ,
                cancellationToken);

        return EmployeeManagementResult<EmployeeResponse>
            .Success(
                responses.Single());
    }
}