using Mawasem.Application.Features.Authentication.Contracts.Requests;
using Mawasem.Application.Features.Authentication.Contracts.Responses;
using Mawasem.Application.Features.Authentication.Interfaces;
using Mawasem.Application.Features.Authentication.Models;
using Mawasem.Domain.Identity;
using Mawasem.Infrastructure.Persistence.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Mawasem.Infrastructure.Authentication;

public sealed class DashboardAuthenticationService
    : IDashboardAuthenticationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly MawasemDbContext _dbContext;
    private readonly ITokenService _tokenService;
    private readonly TimeProvider _timeProvider;

    public DashboardAuthenticationService(
        UserManager<ApplicationUser> userManager ,
        MawasemDbContext dbContext ,
        ITokenService tokenService ,
        TimeProvider timeProvider )
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _tokenService = tokenService;
        _timeProvider = timeProvider;
    }

    public async Task<DashboardAuthenticationSessionResult> LoginAsync(
        LoginAdminRequest request ,
        string? ipAddress ,
        CancellationToken cancellationToken = default )
    {
        ArgumentNullException.ThrowIfNull(request);

        if ( string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password) )
        {
            return InvalidCredentials();
        }

        var normalizedEmail =
            _userManager.NormalizeEmail(
                request.Email.Trim());

        if ( string.IsNullOrWhiteSpace(normalizedEmail) )
        {
            return InvalidCredentials();
        }

        var matchingUsers =
            await _dbContext.Users
                .Where(user =>
                    user.NormalizedEmail == normalizedEmail)
                .Take(2)
                .ToListAsync(cancellationToken);

        // More than one matching account is treated as invalid rather
        // than selecting an account ambiguously.
        if ( matchingUsers.Count != 1 )
        {
            return InvalidCredentials();
        }

        var user = matchingUsers[0];

        if ( user.IsBlocked )
        {
            return AuthenticationSessionResultFailure(
                AuthenticationErrorCodes.AccountBlocked ,
                "This dashboard account has been blocked.");
        }

        var roles =
            await _userManager.GetRolesAsync(user);

        var hasDashboardRole =
            roles.Any(SystemRoles.IsDashboardRole);

        if ( !hasDashboardRole )
        {
            return InvalidCredentials();
        }

        if ( _userManager.SupportsUserLockout &&
            await _userManager.IsLockedOutAsync(user) )
        {
            return AccountLocked();
        }

        var passwordIsCorrect =
            await _userManager.CheckPasswordAsync(
                user ,
                request.Password);

        if ( !passwordIsCorrect )
        {
            if ( _userManager.SupportsUserLockout )
            {
                var accessFailedResult =
                    await _userManager.AccessFailedAsync(user);

                if ( !accessFailedResult.Succeeded )
                {
                    return AuthenticationSessionResultFailure(
                        AuthenticationErrorCodes.InvalidCredentials ,
                        "The login attempt could not be completed.");
                }

                if ( await _userManager.IsLockedOutAsync(user) )
                {
                    return AccountLocked();
                }
            }

            return InvalidCredentials();
        }

        if ( _userManager.SupportsUserLockout )
        {
            var resetFailureCountResult =
                await _userManager
                    .ResetAccessFailedCountAsync(user);

            if ( !resetFailureCountResult.Succeeded )
            {
                return AuthenticationSessionResultFailure(
                    AuthenticationErrorCodes.InvalidCredentials ,
                    "The login attempt could not be completed.");
            }
        }

        return await CreateSessionAsync(
            user ,
            ipAddress ,
            cancellationToken);
    }

    public async Task<DashboardAuthenticationSessionResult> RefreshAsync(
        string? refreshToken ,
        string? ipAddress ,
        CancellationToken cancellationToken = default )
    {
        if ( string.IsNullOrWhiteSpace(refreshToken) )
        {
            return InvalidRefreshToken();
        }

        var tokenHash =
            _tokenService.HashRefreshToken(refreshToken);

        var storedToken =
            await _dbContext.RefreshTokens
                .Include(token => token.User)
                .SingleOrDefaultAsync(
                    token => token.TokenHash == tokenHash ,
                    cancellationToken);

        if ( storedToken is null )
        {
            return InvalidRefreshToken();
        }

        var now =
            _timeProvider.GetUtcNow().UtcDateTime;

        if ( storedToken.RevokedAtUtc.HasValue )
        {
            if ( !string.IsNullOrWhiteSpace(
                    storedToken.ReplacedByTokenHash) )
            {
                await RevokeAllActiveTokensAsync(
                    storedToken.UserId ,
                    now ,
                    ipAddress ,
                    "Dashboard refresh token reuse detected." ,
                    cancellationToken);

                await _dbContext.SaveChangesAsync(
                    cancellationToken);
            }

            return InvalidRefreshToken();
        }

        if ( storedToken.ExpiresAtUtc <= now )
        {
            return InvalidRefreshToken();
        }

        var user = storedToken.User;

        if ( user.IsBlocked )
        {
            await RevokeAllActiveTokensAsync(
                user.Id ,
                now ,
                ipAddress ,
                "Dashboard account blocked." ,
                cancellationToken);

            await _dbContext.SaveChangesAsync(
                cancellationToken);

            return AuthenticationSessionResultFailure(
                AuthenticationErrorCodes.AccountBlocked ,
                "This dashboard account has been blocked.");
        }

        var roles =
            await _userManager.GetRolesAsync(user);

        var hasDashboardRole =
            roles.Any(SystemRoles.IsDashboardRole);

        if ( !hasDashboardRole )
        {
            return InvalidRefreshToken();
        }

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(
                cancellationToken);

        var replacementToken =
            _tokenService.CreateRefreshToken();

        storedToken.RevokedAtUtc = now;
        storedToken.RevokedByIp =
            NormalizeIpAddress(ipAddress);

        storedToken.ReplacedByTokenHash =
            replacementToken.TokenHash;

        storedToken.RevocationReason =
            "Replaced during dashboard refresh-token rotation.";

        _dbContext.RefreshTokens.Add(
            new RefreshToken
            {
                UserId = user.Id ,
                TokenHash = replacementToken.TokenHash ,
                CreatedAtUtc = now ,
                ExpiresAtUtc =
                    replacementToken.ExpiresAtUtc ,
                CreatedByIp =
                    NormalizeIpAddress(ipAddress)
            });

        var response =
            await CreateAuthenticationResponseAsync(
                user ,
                cancellationToken);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        await transaction.CommitAsync(
            cancellationToken);

        return DashboardAuthenticationSessionResult.Success(
            response ,
            replacementToken.Token);
    }

    public async Task LogoutAsync(
        string? refreshToken ,
        string? ipAddress ,
        CancellationToken cancellationToken = default )
    {
        if ( string.IsNullOrWhiteSpace(refreshToken) )
        {
            return;
        }

        var tokenHash =
            _tokenService.HashRefreshToken(refreshToken);

        var storedToken =
            await _dbContext.RefreshTokens
                .SingleOrDefaultAsync(
                    token => token.TokenHash == tokenHash ,
                    cancellationToken);

        if ( storedToken is null ||
            storedToken.RevokedAtUtc.HasValue )
        {
            return;
        }

        storedToken.RevokedAtUtc =
            _timeProvider.GetUtcNow().UtcDateTime;

        storedToken.RevokedByIp =
            NormalizeIpAddress(ipAddress);

        storedToken.RevocationReason =
            "Dashboard user logged out.";

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private async Task<DashboardAuthenticationSessionResult>
        CreateSessionAsync(
            ApplicationUser user ,
            string? ipAddress ,
            CancellationToken cancellationToken )
    {
        var refreshToken =
            _tokenService.CreateRefreshToken();

        var now =
            _timeProvider.GetUtcNow().UtcDateTime;

        _dbContext.RefreshTokens.Add(
            new RefreshToken
            {
                UserId = user.Id ,
                TokenHash = refreshToken.TokenHash ,
                CreatedAtUtc = now ,
                ExpiresAtUtc =
                    refreshToken.ExpiresAtUtc ,
                CreatedByIp =
                    NormalizeIpAddress(ipAddress)
            });

        var response =
            await CreateAuthenticationResponseAsync(
                user ,
                cancellationToken);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return DashboardAuthenticationSessionResult.Success(
            response ,
            refreshToken.Token);
    }

    private async Task<DashboardAuthenticationResponse>
        CreateAuthenticationResponseAsync(
            ApplicationUser user ,
            CancellationToken cancellationToken )
    {
        var accessToken =
            await _tokenService.CreateAccessTokenAsync(
                user ,
                cancellationToken);

        var assignedRoles =
            await _userManager.GetRolesAsync(user);

        var dashboardRoles =
            assignedRoles
                .Where(SystemRoles.IsDashboardRole)
                .OrderBy(role => role)
                .ToArray();

        var roleIds =
            await _dbContext.Roles
                .Where(role =>
                    role.Name != null &&
                    dashboardRoles.Contains(role.Name))
                .Select(role => role.Id)
                .ToListAsync(cancellationToken);

        var permissions =
            await (
                from rolePermission
                    in _dbContext.RolePermissions
                join permission
                    in _dbContext.Permissions
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

        return new DashboardAuthenticationResponse
        {
            AccessToken = accessToken.Token ,

            AccessTokenExpiresAtUtc =
                accessToken.ExpiresAtUtc ,

            User = new DashboardAuthenticatedUserResponse
            {
                Id = user.Id ,
                FullNameAr = user.FullNameAr ,
                FullNameEn = user.FullNameEn ,
                Email = user.Email ?? string.Empty ,

                MustChangePassword =
                    user.MustChangePassword ,

                Roles = dashboardRoles ,
                Permissions = permissions
            }
        };
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
                    token.ExpiresAtUtc > revokedAtUtc)
                .ToListAsync(cancellationToken);

        var normalizedIpAddress =
            NormalizeIpAddress(ipAddress);

        foreach ( var token in activeTokens )
        {
            token.RevokedAtUtc = revokedAtUtc;
            token.RevokedByIp = normalizedIpAddress;
            token.RevocationReason = reason;
        }
    }

    private static DashboardAuthenticationSessionResult
        InvalidCredentials()
    {
        return AuthenticationSessionResultFailure(
            AuthenticationErrorCodes.InvalidCredentials ,
            "The email or password is incorrect.");
    }

    private static DashboardAuthenticationSessionResult
        InvalidRefreshToken()
    {
        return AuthenticationSessionResultFailure(
            AuthenticationErrorCodes.InvalidRefreshToken ,
            "The dashboard refresh token is invalid or has expired.");
    }

    private static DashboardAuthenticationSessionResult
        AccountLocked()
    {
        return AuthenticationSessionResultFailure(
            AuthenticationErrorCodes.AccountLocked ,
            "The account is temporarily locked because of repeated failed login attempts.");
    }

    private static DashboardAuthenticationSessionResult
        AuthenticationSessionResultFailure(
            string errorCode ,
            string errorMessage )
    {
        return DashboardAuthenticationSessionResult.Failure(
            errorCode ,
            errorMessage);
    }

    private static string? NormalizeIpAddress(
        string? ipAddress )
    {
        if ( string.IsNullOrWhiteSpace(ipAddress) )
        {
            return null;
        }

        var trimmedValue = ipAddress.Trim();

        return trimmedValue.Length <= 45
            ? trimmedValue
            : trimmedValue[..45];
    }
}