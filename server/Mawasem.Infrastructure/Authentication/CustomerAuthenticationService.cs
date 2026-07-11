using Mawasem.Application.Features.Authentication.Contracts.Requests;
using Mawasem.Application.Features.Authentication.Contracts.Responses;
using Mawasem.Application.Features.Authentication.Interfaces;
using Mawasem.Application.Features.Authentication.Models;
using Mawasem.Domain.Identity;
using Mawasem.Infrastructure.Persistence.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Mawasem.Infrastructure.Authentication;

public sealed class CustomerAuthenticationService
    : ICustomerAuthenticationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly MawasemDbContext _dbContext;
    private readonly ITokenService _tokenService;
    private readonly TimeProvider _timeProvider;

    public CustomerAuthenticationService(
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

    public async Task<AuthenticationSessionResult> RegisterAsync(
        RegisterCustomerRequest request ,
        string? ipAddress ,
        CancellationToken cancellationToken = default )
    {
        ArgumentNullException.ThrowIfNull(request);

        var validationFailure = ValidateRegistration(request);

        if ( validationFailure is not null )
        {
            return validationFailure;
        }

        if ( !EgyptianPhoneNumberNormalizer.TryNormalize(
                request.PhoneNumber ,
                out var normalizedPhoneNumber) )
        {
            return AuthenticationSessionResult.Failure(
                AuthenticationErrorCodes.InvalidPhoneNumber ,
                "Enter a valid Egyptian mobile phone number.");
        }

        var phoneNumberExists =
            await _dbContext.Users.AnyAsync(
                user =>
                    user.PhoneNumber == normalizedPhoneNumber ||
                    user.UserName == normalizedPhoneNumber ,
                cancellationToken);

        if ( phoneNumberExists )
        {
            return AuthenticationSessionResult.Failure(
                AuthenticationErrorCodes.PhoneAlreadyRegistered ,
                "This phone number is already registered.");
        }

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(
                cancellationToken);

        try
        {
            var user = new ApplicationUser
            {
                UserName = normalizedPhoneNumber ,
                PhoneNumber = normalizedPhoneNumber ,
                PhoneNumberConfirmed = false ,

                FullNameAr = request.FullNameAr.Trim() ,
                FullNameEn = request.FullNameEn.Trim() ,

                BirthDate = request.BirthDate ,
                Gender = request.Gender ,
                ReferralSource = request.ReferralSource ,

                IsBlocked = false
            };

            var createResult =
                await _userManager.CreateAsync(
                    user ,
                    request.Password);

            if ( !createResult.Succeeded )
            {
                return AuthenticationSessionResult.Failure(
                    AuthenticationErrorCodes.RegistrationFailed ,
                    JoinIdentityErrors(createResult));
            }

            var roleResult =
                await _userManager.AddToRoleAsync(
                    user ,
                    SystemRoles.Customer);

            if ( !roleResult.Succeeded )
            {
                return AuthenticationSessionResult.Failure(
                    AuthenticationErrorCodes.RegistrationFailed ,
                    JoinIdentityErrors(roleResult));
            }

            var sessionResult =
                await CreateSessionAsync(
                    user ,
                    ipAddress ,
                    cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return sessionResult;
        }
        catch ( DbUpdateException exception )
            when ( IsUniqueConstraintViolation(exception) )
        {
            return AuthenticationSessionResult.Failure(
                AuthenticationErrorCodes.PhoneAlreadyRegistered ,
                "This phone number is already registered.");
        }
    }

    public async Task<AuthenticationSessionResult> LoginAsync(
        LoginCustomerRequest request ,
        string? ipAddress ,
        CancellationToken cancellationToken = default )
    {
        ArgumentNullException.ThrowIfNull(request);

        if ( string.IsNullOrWhiteSpace(request.Password) ||
            !EgyptianPhoneNumberNormalizer.TryNormalize(
                request.PhoneNumber ,
                out var normalizedPhoneNumber) )
        {
            return InvalidCredentials();
        }

        var user =
            await _dbContext.Users.SingleOrDefaultAsync(
                customer =>
                    customer.PhoneNumber == normalizedPhoneNumber ,
                cancellationToken);

        if ( user is null )
        {
            return InvalidCredentials();
        }

        if ( user.IsBlocked )
        {
            return AuthenticationSessionResult.Failure(
                AuthenticationErrorCodes.AccountBlocked ,
                "This account has been blocked.");
        }

        var isCustomer =
            await _userManager.IsInRoleAsync(
                user ,
                SystemRoles.Customer);

        if ( !isCustomer )
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
                    return AuthenticationSessionResult.Failure(
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
                await _userManager.ResetAccessFailedCountAsync(user);

            if ( !resetFailureCountResult.Succeeded )
            {
                return AuthenticationSessionResult.Failure(
                    AuthenticationErrorCodes.InvalidCredentials ,
                    "The login attempt could not be completed.");
            }
        }

        return await CreateSessionAsync(
            user ,
            ipAddress ,
            cancellationToken);
    }

    public async Task<AuthenticationSessionResult> RefreshAsync(
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
            // A revoked token that has already been replaced may indicate
            // that an old token has been copied and reused.
            if ( !string.IsNullOrWhiteSpace(
                    storedToken.ReplacedByTokenHash) )
            {
                await RevokeAllActiveTokensAsync(
                    storedToken.UserId ,
                    now ,
                    ipAddress ,
                    "Refresh token reuse detected." ,
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
                "Account blocked." ,
                cancellationToken);

            await _dbContext.SaveChangesAsync(
                cancellationToken);

            return AuthenticationSessionResult.Failure(
                AuthenticationErrorCodes.AccountBlocked ,
                "This account has been blocked.");
        }

        var isCustomer =
            await _userManager.IsInRoleAsync(
                user ,
                SystemRoles.Customer);

        if ( !isCustomer )
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
            "Replaced during refresh-token rotation.";

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

        return AuthenticationSessionResult.Success(
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
            "Customer logged out.";

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private async Task<AuthenticationSessionResult> CreateSessionAsync(
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
                ExpiresAtUtc = refreshToken.ExpiresAtUtc ,
                CreatedByIp = NormalizeIpAddress(ipAddress)
            });

        var response =
            await CreateAuthenticationResponseAsync(
                user ,
                cancellationToken);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return AuthenticationSessionResult.Success(
            response ,
            refreshToken.Token);
    }

    private async Task<AuthenticationResponse>
        CreateAuthenticationResponseAsync(
            ApplicationUser user ,
            CancellationToken cancellationToken )
    {
        var accessToken =
            await _tokenService.CreateAccessTokenAsync(
                user ,
                cancellationToken);

        var roles =
            await _userManager.GetRolesAsync(user);

        return new AuthenticationResponse
        {
            AccessToken = accessToken.Token ,
            AccessTokenExpiresAtUtc =
                accessToken.ExpiresAtUtc ,

            User = new AuthenticatedUserResponse
            {
                Id = user.Id ,
                FullNameAr = user.FullNameAr ,
                FullNameEn = user.FullNameEn ,
                PhoneNumber = user.PhoneNumber ,
                Email = user.Email ,
                Roles = roles.ToArray()
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

    private AuthenticationSessionResult? ValidateRegistration(
        RegisterCustomerRequest request )
    {
        if ( string.IsNullOrWhiteSpace(request.FullNameAr) )
        {
            return InvalidRequest(
                "The Arabic full name is required.");
        }

        if ( string.IsNullOrWhiteSpace(request.FullNameEn) )
        {
            return InvalidRequest(
                "The English full name is required.");
        }

        if ( request.FullNameAr.Trim().Length > 200 ||
            request.FullNameEn.Trim().Length > 200 )
        {
            return InvalidRequest(
                "A full name cannot exceed 200 characters.");
        }

        if ( request.Password != request.ConfirmPassword )
        {
            return InvalidRequest(
                "Password and confirmation password do not match.");
        }

        if ( !Enum.IsDefined(request.Gender) )
        {
            return InvalidRequest(
                "Select a valid gender.");
        }

        if ( !Enum.IsDefined(request.ReferralSource) )
        {
            return InvalidRequest(
                "Select a valid referral source.");
        }

        if ( request.BirthDate.HasValue )
        {
            var today =
                DateOnly.FromDateTime(
                    _timeProvider
                        .GetUtcNow()
                        .UtcDateTime);

            if ( request.BirthDate.Value > today )
            {
                return InvalidRequest(
                    "Birth date cannot be in the future.");
            }
        }

        return null;
    }

    private static AuthenticationSessionResult InvalidRequest(
        string message )
    {
        return AuthenticationSessionResult.Failure(
            AuthenticationErrorCodes.InvalidRequest ,
            message);
    }

    private static AuthenticationSessionResult InvalidCredentials()
    {
        return AuthenticationSessionResult.Failure(
            AuthenticationErrorCodes.InvalidCredentials ,
            "The phone number or password is incorrect.");
    }

    private static AuthenticationSessionResult InvalidRefreshToken()
    {
        return AuthenticationSessionResult.Failure(
            AuthenticationErrorCodes.InvalidRefreshToken ,
            "The refresh token is invalid or has expired.");
    }

    private static AuthenticationSessionResult AccountLocked()
    {
        return AuthenticationSessionResult.Failure(
            AuthenticationErrorCodes.AccountLocked ,
            "The account is temporarily locked because of repeated failed login attempts.");
    }

    private static string JoinIdentityErrors(
        IdentityResult result )
    {
        return string.Join(
            "; " ,
            result.Errors.Select(error => error.Description));
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

    private static bool IsUniqueConstraintViolation(
        DbUpdateException exception )
    {
        return exception.InnerException is SqlException
        {
            Number: 2601 or 2627
        };
    }
}