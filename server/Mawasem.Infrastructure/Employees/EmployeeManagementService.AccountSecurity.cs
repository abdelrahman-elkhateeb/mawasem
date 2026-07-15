using Mawasem.Application.Features.Employees.Contracts.Requests;
using Mawasem.Application.Features.Employees.Models;

namespace Mawasem.Infrastructure.Employees;

public sealed partial class EmployeeManagementService
{
    public async Task<EmployeeManagementOperationResult>
        BlockAsync(
            int actorUserId ,
            int employeeId ,
            BlockEmployeeRequest request ,
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
            return EmployeeManagementOperationResult.Failure(
                EmployeeManagementErrorCodes.Forbidden ,
                "The authenticated account cannot block dashboard employees.");
        }

        var managedEmployee =
            await GetManagedEmployeeAsync(
                employeeId ,
                cancellationToken);

        if ( managedEmployee is null )
        {
            return EmployeeManagementOperationResult.Failure(
                EmployeeManagementErrorCodes.NotFound ,
                "The dashboard employee was not found.");
        }

        if ( managedEmployee.Roles.Contains(
                Domain.Identity.SystemRoles.SuperAdmin ,
                StringComparer.OrdinalIgnoreCase) )
        {
            return EmployeeManagementOperationResult.Failure(
                EmployeeManagementErrorCodes
                    .CannotManageSuperAdmin ,
                "The SuperAdmin account cannot be blocked.");
        }

        if ( actorUserId == employeeId )
        {
            return EmployeeManagementOperationResult.Failure(
                EmployeeManagementErrorCodes.CannotManageSelf ,
                "You cannot block your own dashboard account.");
        }

        if ( !actorAccess.IsSuperAdmin &&
            managedEmployee.Roles.Contains(
                Domain.Identity.SystemRoles.Admin ,
                StringComparer.OrdinalIgnoreCase) )
        {
            return EmployeeManagementOperationResult.Failure(
                EmployeeManagementErrorCodes.Forbidden ,
                "Only a SuperAdmin can block an Admin account.");
        }

        if ( !TryValidateBlockReason(
                request.Reason ,
                out var blockReason ,
                out var reasonError) )
        {
            return EmployeeManagementOperationResult.Failure(
                EmployeeManagementErrorCodes.InvalidRequest ,
                reasonError);
        }

        var now =
            _timeProvider.GetUtcNow().UtcDateTime;

        managedEmployee.User.IsBlocked = true;
        managedEmployee.User.BlockedAt = now;
        managedEmployee.User.BlockedReason =
            blockReason;

        await RevokeAllActiveTokensAsync(
            managedEmployee.User.Id ,
            now ,
            ipAddress ,
            "Dashboard account blocked." ,
            cancellationToken);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return EmployeeManagementOperationResult.Success();
    }

    public async Task<EmployeeManagementOperationResult>
        UnblockAsync(
            int actorUserId ,
            int employeeId ,
            CancellationToken cancellationToken = default )
    {
        var actorAccess =
            await GetActorAccessAsync(
                actorUserId ,
                cancellationToken);

        if ( actorAccess is null )
        {
            return EmployeeManagementOperationResult.Failure(
                EmployeeManagementErrorCodes.Forbidden ,
                "The authenticated account cannot unblock dashboard employees.");
        }

        var managedEmployee =
            await GetManagedEmployeeAsync(
                employeeId ,
                cancellationToken);

        if ( managedEmployee is null )
        {
            return EmployeeManagementOperationResult.Failure(
                EmployeeManagementErrorCodes.NotFound ,
                "The dashboard employee was not found.");
        }

        if ( managedEmployee.Roles.Contains(
                Domain.Identity.SystemRoles.SuperAdmin ,
                StringComparer.OrdinalIgnoreCase) )
        {
            return EmployeeManagementOperationResult.Failure(
                EmployeeManagementErrorCodes
                    .CannotManageSuperAdmin ,
                "The SuperAdmin account cannot be unblocked through employee management.");
        }

        if ( actorUserId == employeeId )
        {
            return EmployeeManagementOperationResult.Failure(
                EmployeeManagementErrorCodes.CannotManageSelf ,
                "You cannot unblock your own dashboard account.");
        }

        if ( !actorAccess.IsSuperAdmin &&
            managedEmployee.Roles.Contains(
                Domain.Identity.SystemRoles.Admin ,
                StringComparer.OrdinalIgnoreCase) )
        {
            return EmployeeManagementOperationResult.Failure(
                EmployeeManagementErrorCodes.Forbidden ,
                "Only a SuperAdmin can unblock an Admin account.");
        }

        managedEmployee.User.IsBlocked = false;
        managedEmployee.User.BlockedAt = null;
        managedEmployee.User.BlockedReason = null;
        managedEmployee.User.LockoutEnd = null;
        managedEmployee.User.AccessFailedCount = 0;

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return EmployeeManagementOperationResult.Success();
    }

    public async Task<EmployeeManagementOperationResult>
        ResetPasswordAsync(
            int actorUserId ,
            int employeeId ,
            ResetEmployeePasswordRequest request ,
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
            return EmployeeManagementOperationResult.Failure(
                EmployeeManagementErrorCodes.Forbidden ,
                "The authenticated account cannot reset employee passwords.");
        }

        var managedEmployee =
            await GetManagedEmployeeAsync(
                employeeId ,
                cancellationToken);

        if ( managedEmployee is null )
        {
            return EmployeeManagementOperationResult.Failure(
                EmployeeManagementErrorCodes.NotFound ,
                "The dashboard employee was not found.");
        }

        if ( managedEmployee.Roles.Contains(
                Domain.Identity.SystemRoles.SuperAdmin ,
                StringComparer.OrdinalIgnoreCase) )
        {
            return EmployeeManagementOperationResult.Failure(
                EmployeeManagementErrorCodes
                    .CannotManageSuperAdmin ,
                "The SuperAdmin password cannot be reset through employee management.");
        }

        if ( actorUserId == employeeId )
        {
            return EmployeeManagementOperationResult.Failure(
                EmployeeManagementErrorCodes.CannotManageSelf ,
                "Use the dashboard change-password endpoint to change your own password.");
        }

        if ( !actorAccess.IsSuperAdmin &&
            managedEmployee.Roles.Contains(
                Domain.Identity.SystemRoles.Admin ,
                StringComparer.OrdinalIgnoreCase) )
        {
            return EmployeeManagementOperationResult.Failure(
                EmployeeManagementErrorCodes.Forbidden ,
                "Only a SuperAdmin can reset an Admin password.");
        }

        if ( !TryValidateTemporaryPassword(
                request.TemporaryPassword ,
                request.ConfirmTemporaryPassword ,
                out var passwordErrorCode ,
                out var passwordError) )
        {
            return EmployeeManagementOperationResult.Failure(
                passwordErrorCode ,
                passwordError);
        }

        await using var transaction =
            await _dbContext.Database
                .BeginTransactionAsync(
                    cancellationToken);

        var resetToken =
            await _userManager
                .GeneratePasswordResetTokenAsync(
                    managedEmployee.User);

        var resetResult =
            await _userManager.ResetPasswordAsync(
                managedEmployee.User ,
                resetToken ,
                request.TemporaryPassword);

        if ( !resetResult.Succeeded )
        {
            return EmployeeManagementOperationResult.Failure(
                EmployeeManagementErrorCodes
                    .PasswordResetFailed ,
                JoinIdentityErrors(
                    resetResult.Errors));
        }

        managedEmployee.User.MustChangePassword = true;
        managedEmployee.User.LockoutEnd = null;
        managedEmployee.User.AccessFailedCount = 0;

        var now =
            _timeProvider.GetUtcNow().UtcDateTime;

        await RevokeAllActiveTokensAsync(
            managedEmployee.User.Id ,
            now ,
            ipAddress ,
            "Dashboard password reset by an administrator." ,
            cancellationToken);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        await transaction.CommitAsync(
            cancellationToken);

        return EmployeeManagementOperationResult.Success();
    }
}