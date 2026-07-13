using Mawasem.Application.Features.Authentication.Contracts.Requests;
using Mawasem.Application.Features.Authentication.Contracts.Responses;
using Mawasem.Application.Features.Authentication.Interfaces;
using Mawasem.Application.Features.Authentication.Models;
using Mawasem.Application.Features.Authentication.Options;
using Mawasem.Domain.Enums;
using Mawasem.Domain.Identity;
using Mawasem.Infrastructure.Persistence.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace Mawasem.Infrastructure.Authentication;

public sealed class CustomerPasswordResetService
    : ICustomerPasswordResetService
{
    private const PasswordResetChannel ResetChannel =
        PasswordResetChannel.Sms;

    private const string RefreshTokenRevocationReason =
        "Customer password reset.";

    private readonly UserManager<ApplicationUser> _userManager;

    private readonly IPasswordHasher<ApplicationUser>
        _passwordHasher;

    private readonly MawasemDbContext _dbContext;

    private readonly ICustomerPasswordResetCodeSender
        _codeSender;

    private readonly CustomerPasswordResetOptions _options;

    private readonly TimeProvider _timeProvider;

    private readonly ILogger<CustomerPasswordResetService>
        _logger;

    public CustomerPasswordResetService(
        UserManager<ApplicationUser> userManager ,
        IPasswordHasher<ApplicationUser> passwordHasher ,
        MawasemDbContext dbContext ,
        ICustomerPasswordResetCodeSender codeSender ,
        IOptions<CustomerPasswordResetOptions> options ,
        TimeProvider timeProvider ,
        ILogger<CustomerPasswordResetService> logger )
    {
        _userManager = userManager;
        _passwordHasher = passwordHasher;
        _dbContext = dbContext;
        _codeSender = codeSender;
        _options = options.Value;
        _timeProvider = timeProvider;
        _logger = logger;

        ValidateOptions(_options);
    }

    public async Task<CustomerPasswordResetOperationResult>
        RequestCodeAsync(
            ForgotCustomerPasswordRequest request ,
            string? ipAddress ,
            CancellationToken cancellationToken = default )
    {
        if ( request is null ||
            string.IsNullOrWhiteSpace(request.PhoneNumber) )
        {
            return CustomerPasswordResetOperationResult.Failure(
                AuthenticationErrorCodes.InvalidRequest ,
                "A phone number is required.");
        }

        if ( !EgyptianPhoneNumberNormalizer.TryNormalize(
                request.PhoneNumber ,
                out var normalizedPhoneNumber) )
        {
            return CustomerPasswordResetOperationResult.Failure(
                AuthenticationErrorCodes.InvalidPhoneNumber ,
                "Enter a valid Egyptian mobile phone number.");
        }

        var user =
            await FindUniqueUserByPhoneNumberAsync(
                normalizedPhoneNumber ,
                cancellationToken);

        // Always return success when the account does not exist,
        // is blocked, or is not a customer. This prevents account
        // discovery through the forgot-password endpoint.
        if ( user is null ||
            user.IsBlocked ||
            !await _userManager.IsInRoleAsync(
                user ,
                SystemRoles.Customer) )
        {
            return CustomerPasswordResetOperationResult.Success();
        }

        var now =
            _timeProvider.GetUtcNow().UtcDateTime;

        var latestOutstandingCode =
            await _dbContext.PasswordResetCodes
                .Where(resetCode =>
                    resetCode.UserId == user.Id &&
                    resetCode.Channel == ResetChannel &&
                    resetCode.UsedAtUtc == null &&
                    resetCode.RevokedAtUtc == null)
                .OrderByDescending(resetCode =>
                    resetCode.CreatedAtUtc)
                .FirstOrDefaultAsync(cancellationToken);

        if ( latestOutstandingCode is not null )
        {
            var resendAllowedAtUtc =
                latestOutstandingCode.CreatedAtUtc.AddSeconds(
                    _options.ResendCooldownSeconds);

            if ( now < resendAllowedAtUtc )
            {
                return CustomerPasswordResetOperationResult
                    .Success();
            }
        }

        var outstandingCodes =
            await _dbContext.PasswordResetCodes
                .Where(resetCode =>
                    resetCode.UserId == user.Id &&
                    resetCode.Channel == ResetChannel &&
                    resetCode.UsedAtUtc == null &&
                    resetCode.RevokedAtUtc == null)
                .ToListAsync(cancellationToken);

        foreach ( var outstandingCode in outstandingCodes )
        {
            outstandingCode.RevokedAtUtc = now;
        }

        var verificationCode =
            GenerateVerificationCode(_options.CodeLength);

        var expiresAtUtc =
            now.AddMinutes(_options.CodeLifetimeMinutes);

        var passwordResetCode =
            new PasswordResetCode
            {
                UserId = user.Id ,
                Channel = ResetChannel ,

                CodeHash =
                    _passwordHasher.HashPassword(
                        user ,
                        verificationCode) ,

                CreatedAtUtc = now ,
                ExpiresAtUtc = expiresAtUtc ,
                FailedAttempts = 0 ,
                RequestedByIp = NormalizeIpAddress(ipAddress)
            };

        _dbContext.PasswordResetCodes.Add(
            passwordResetCode);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        try
        {
            await _codeSender.SendAsync(
                normalizedPhoneNumber ,
                verificationCode ,
                expiresAtUtc ,
                cancellationToken);
        }
        catch ( Exception exception )
        {
            _logger.LogError(
                exception ,
                "Customer password reset code delivery failed " +
                "for user {UserId}." ,
                user.Id);

            // Revoke the undelivered code while returning the
            // generic response to avoid revealing the account.
            try
            {
                passwordResetCode.RevokedAtUtc =
                    _timeProvider.GetUtcNow().UtcDateTime;

                await _dbContext.SaveChangesAsync(
                    cancellationToken);
            }
            catch ( Exception revocationException )
            {
                _logger.LogError(
                    revocationException ,
                    "Failed to revoke an undelivered customer " +
                    "password reset code {PasswordResetCodeId}." ,
                    passwordResetCode.Id);
            }
        }

        return CustomerPasswordResetOperationResult.Success();
    }

    public async Task<CustomerPasswordResetVerificationResult>
        VerifyCodeAsync(
            VerifyCustomerPasswordResetCodeRequest request ,
            CancellationToken cancellationToken = default )
    {
        if ( request is null ||
            string.IsNullOrWhiteSpace(request.PhoneNumber) ||
            string.IsNullOrWhiteSpace(request.Code) )
        {
            return InvalidVerificationCodeResult();
        }

        if ( !EgyptianPhoneNumberNormalizer.TryNormalize(
                request.PhoneNumber ,
                out var normalizedPhoneNumber) )
        {
            return InvalidVerificationCodeResult();
        }

        var suppliedCode =
            request.Code.Trim();

        if ( !IsValidVerificationCode(suppliedCode) )
        {
            return InvalidVerificationCodeResult();
        }

        var user =
            await FindUniqueUserByPhoneNumberAsync(
                normalizedPhoneNumber ,
                cancellationToken);

        if ( user is null ||
            user.IsBlocked ||
            !await _userManager.IsInRoleAsync(
                user ,
                SystemRoles.Customer) )
        {
            return InvalidVerificationCodeResult();
        }

        var now =
            _timeProvider.GetUtcNow().UtcDateTime;

        var passwordResetCode =
            await _dbContext.PasswordResetCodes
                .Where(resetCode =>
                    resetCode.UserId == user.Id &&
                    resetCode.Channel == ResetChannel &&
                    resetCode.VerifiedAtUtc == null &&
                    resetCode.UsedAtUtc == null &&
                    resetCode.RevokedAtUtc == null)
                .OrderByDescending(resetCode =>
                    resetCode.CreatedAtUtc)
                .FirstOrDefaultAsync(cancellationToken);

        if ( passwordResetCode is null )
        {
            return InvalidVerificationCodeResult();
        }

        if ( now >= passwordResetCode.ExpiresAtUtc )
        {
            passwordResetCode.RevokedAtUtc = now;

            await _dbContext.SaveChangesAsync(
                cancellationToken);

            return InvalidVerificationCodeResult();
        }

        if ( passwordResetCode.FailedAttempts >=
            _options.MaxFailedAttempts )
        {
            passwordResetCode.RevokedAtUtc = now;

            await _dbContext.SaveChangesAsync(
                cancellationToken);

            return InvalidVerificationCodeResult();
        }

        var verificationResult =
            _passwordHasher.VerifyHashedPassword(
                user ,
                passwordResetCode.CodeHash ,
                suppliedCode);

        if ( verificationResult ==
            PasswordVerificationResult.Failed )
        {
            passwordResetCode.FailedAttempts++;

            if ( passwordResetCode.FailedAttempts >=
                _options.MaxFailedAttempts )
            {
                passwordResetCode.RevokedAtUtc = now;
            }

            await _dbContext.SaveChangesAsync(
                cancellationToken);

            return InvalidVerificationCodeResult();
        }

        if ( verificationResult ==
            PasswordVerificationResult.SuccessRehashNeeded )
        {
            passwordResetCode.CodeHash =
                _passwordHasher.HashPassword(
                    user ,
                    suppliedCode);
        }

        var resetToken =
            GenerateResetToken();

        var resetTokenExpiresAtUtc =
            now.AddMinutes(
                _options.ResetTokenLifetimeMinutes);

        passwordResetCode.VerifiedAtUtc = now;

        passwordResetCode.ResetTokenHash =
            HashResetToken(resetToken);

        passwordResetCode.ResetTokenExpiresAtUtc =
            resetTokenExpiresAtUtc;

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return CustomerPasswordResetVerificationResult.Success(
            new CustomerPasswordResetVerificationResponse
            {
                ResetToken = resetToken ,
                ExpiresAtUtc = resetTokenExpiresAtUtc
            });
    }

    public async Task<CustomerPasswordResetOperationResult>
        ResetPasswordAsync(
            ResetCustomerPasswordRequest request ,
            string? ipAddress ,
            CancellationToken cancellationToken = default )
    {
        if ( request is null ||
            string.IsNullOrWhiteSpace(request.PhoneNumber) ||
            string.IsNullOrWhiteSpace(request.ResetToken) ||
            string.IsNullOrWhiteSpace(request.NewPassword) ||
            string.IsNullOrWhiteSpace(
                request.ConfirmNewPassword) )
        {
            return CustomerPasswordResetOperationResult.Failure(
                AuthenticationErrorCodes.InvalidRequest ,
                "Phone number, reset token, and new password " +
                "are required.");
        }

        if ( !string.Equals(
                request.NewPassword ,
                request.ConfirmNewPassword ,
                StringComparison.Ordinal) )
        {
            return CustomerPasswordResetOperationResult.Failure(
                AuthenticationErrorCodes
                    .PasswordConfirmationMismatch ,

                "The new password and confirmation do not match.");
        }

        if ( !EgyptianPhoneNumberNormalizer.TryNormalize(
                request.PhoneNumber ,
                out var normalizedPhoneNumber) )
        {
            return InvalidResetTokenResult();
        }

        var suppliedResetToken =
            request.ResetToken.Trim();

        if ( suppliedResetToken.Length < 32 )
        {
            return InvalidResetTokenResult();
        }

        var resetTokenHash =
            HashResetToken(suppliedResetToken);

        var passwordResetCode =
            await _dbContext.PasswordResetCodes
                .Include(resetCode => resetCode.User)
                .SingleOrDefaultAsync(
                    resetCode =>
                        resetCode.Channel == ResetChannel &&
                        resetCode.ResetTokenHash ==
                        resetTokenHash ,
                    cancellationToken);

        if ( passwordResetCode is null )
        {
            return InvalidResetTokenResult();
        }

        var user =
            passwordResetCode.User;

        if ( !UserHasPhoneNumber(
                user ,
                normalizedPhoneNumber) )
        {
            return InvalidResetTokenResult();
        }

        var now =
            _timeProvider.GetUtcNow().UtcDateTime;

        if ( !IsResetTokenActive(
                passwordResetCode ,
                now) )
        {
            if ( passwordResetCode.RevokedAtUtc is null &&
                passwordResetCode.UsedAtUtc is null )
            {
                passwordResetCode.RevokedAtUtc = now;

                await _dbContext.SaveChangesAsync(
                    cancellationToken);
            }

            return InvalidResetTokenResult();
        }

        if ( user.IsBlocked ||
            !await _userManager.IsInRoleAsync(
                user ,
                SystemRoles.Customer) )
        {
            return InvalidResetTokenResult();
        }

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(
                cancellationToken);

        try
        {
            var identityResetToken =
                await _userManager
                    .GeneratePasswordResetTokenAsync(user);

            var identityResult =
                await _userManager.ResetPasswordAsync(
                    user ,
                    identityResetToken ,
                    request.NewPassword);

            if ( !identityResult.Succeeded )
            {
                await transaction.RollbackAsync(
                    cancellationToken);

                return CustomerPasswordResetOperationResult
                    .Failure(
                        AuthenticationErrorCodes
                            .PasswordResetFailed ,

                        CreateIdentityErrorMessage(
                            identityResult ,
                            "The password could not be reset."));
            }

            user.AccessFailedCount = 0;
            user.LockoutEnd = null;

            passwordResetCode.UsedAtUtc = now;

            var otherOutstandingResetCodes =
                await _dbContext.PasswordResetCodes
                    .Where(resetCode =>
                        resetCode.UserId == user.Id &&
                        resetCode.Id != passwordResetCode.Id &&
                        resetCode.UsedAtUtc == null &&
                        resetCode.RevokedAtUtc == null)
                    .ToListAsync(cancellationToken);

            foreach ( var otherResetCode
                in otherOutstandingResetCodes )
            {
                otherResetCode.RevokedAtUtc = now;
            }

            var activeRefreshTokens =
                await _dbContext.RefreshTokens
                    .Where(refreshToken =>
                        refreshToken.UserId == user.Id &&
                        refreshToken.RevokedAtUtc == null &&
                        refreshToken.ExpiresAtUtc > now)
                    .ToListAsync(cancellationToken);

            foreach ( var refreshToken in activeRefreshTokens )
            {
                refreshToken.RevokedAtUtc = now;

                refreshToken.RevokedByIp =
                    NormalizeIpAddress(ipAddress);

                refreshToken.RevocationReason =
                    RefreshTokenRevocationReason;
            }

            await _dbContext.SaveChangesAsync(
                cancellationToken);

            await transaction.CommitAsync(
                cancellationToken);

            return CustomerPasswordResetOperationResult
                .Success();
        }
        catch
        {
            await transaction.RollbackAsync(
                cancellationToken);

            throw;
        }
    }

    private async Task<ApplicationUser?>
        FindUniqueUserByPhoneNumberAsync(
            string normalizedPhoneNumber ,
            CancellationToken cancellationToken )
    {
        var users =
            await _userManager.Users
                .Where(user =>
                    user.PhoneNumber ==
                    normalizedPhoneNumber)
                .Take(2)
                .ToListAsync(cancellationToken);

        return users.Count == 1
            ? users[0]
            : null;
    }

    private bool IsValidVerificationCode(
        string code )
    {
        return
            code.Length == _options.CodeLength &&
            code.All(char.IsAsciiDigit);
    }

    private static bool UserHasPhoneNumber(
        ApplicationUser user ,
        string normalizedPhoneNumber )
    {
        return string.Equals(
            user.PhoneNumber ,
            normalizedPhoneNumber ,
            StringComparison.Ordinal);
    }

    private static bool IsResetTokenActive(
        PasswordResetCode passwordResetCode ,
        DateTime now )
    {
        return
            passwordResetCode.VerifiedAtUtc is not null &&
            !string.IsNullOrWhiteSpace(
                passwordResetCode.ResetTokenHash) &&
            passwordResetCode.ResetTokenExpiresAtUtc
                is not null &&
            now <
                passwordResetCode
                    .ResetTokenExpiresAtUtc.Value &&
            passwordResetCode.UsedAtUtc is null &&
            passwordResetCode.RevokedAtUtc is null;
    }

    private static string GenerateVerificationCode(
        int codeLength )
    {
        var maximumValue =
            CalculatePowerOfTen(codeLength);

        var value =
            RandomNumberGenerator.GetInt32(
                0 ,
                maximumValue);

        return value
            .ToString()
            .PadLeft(codeLength , '0');
    }

    private static int CalculatePowerOfTen(
        int exponent )
    {
        var result = 1;

        for ( var index = 0 ;
             index < exponent ;
             index++ )
        {
            result *= 10;
        }

        return result;
    }

    private static string GenerateResetToken()
    {
        var tokenBytes =
            RandomNumberGenerator.GetBytes(32);

        return Convert
            .ToBase64String(tokenBytes)
            .TrimEnd('=')
            .Replace('+' , '-')
            .Replace('/' , '_');
    }

    private static string HashResetToken(
        string resetToken )
    {
        var tokenBytes =
            Encoding.UTF8.GetBytes(resetToken);

        var hashBytes =
            SHA256.HashData(tokenBytes);

        return Convert.ToHexString(hashBytes);
    }

    private static string? NormalizeIpAddress(
        string? ipAddress )
    {
        if ( string.IsNullOrWhiteSpace(ipAddress) )
        {
            return null;
        }

        var trimmedIpAddress =
            ipAddress.Trim();

        return trimmedIpAddress.Length <= 45
            ? trimmedIpAddress
            : trimmedIpAddress[..45];
    }

    private static string CreateIdentityErrorMessage(
        IdentityResult identityResult ,
        string fallbackMessage )
    {
        var descriptions =
            identityResult.Errors
                .Select(error => error.Description)
                .Where(description =>
                    !string.IsNullOrWhiteSpace(description))
                .Distinct()
                .ToArray();

        return descriptions.Length == 0
            ? fallbackMessage
            : string.Join(" " , descriptions);
    }

    private static CustomerPasswordResetVerificationResult
        InvalidVerificationCodeResult()
    {
        return CustomerPasswordResetVerificationResult
            .Failure(
                AuthenticationErrorCodes
                    .PasswordResetCodeInvalid ,

                "The verification code is invalid or expired.");
    }

    private static CustomerPasswordResetOperationResult
        InvalidResetTokenResult()
    {
        return CustomerPasswordResetOperationResult.Failure(
            AuthenticationErrorCodes
                .PasswordResetTokenInvalid ,

            "The password reset token is invalid or expired.");
    }

    private static void ValidateOptions(
        CustomerPasswordResetOptions options )
    {
        if ( options.CodeLength is < 4 or > 9 )
        {
            throw new InvalidOperationException(
                "CustomerPasswordReset:CodeLength must be " +
                "between 4 and 9.");
        }

        if ( options.CodeLifetimeMinutes <= 0 )
        {
            throw new InvalidOperationException(
                "CustomerPasswordReset:CodeLifetimeMinutes " +
                "must be greater than zero.");
        }

        if ( options.ResetTokenLifetimeMinutes <= 0 )
        {
            throw new InvalidOperationException(
                "CustomerPasswordReset:ResetTokenLifetimeMinutes " +
                "must be greater than zero.");
        }

        if ( options.MaxFailedAttempts <= 0 )
        {
            throw new InvalidOperationException(
                "CustomerPasswordReset:MaxFailedAttempts must " +
                "be greater than zero.");
        }

        if ( options.ResendCooldownSeconds < 0 )
        {
            throw new InvalidOperationException(
                "CustomerPasswordReset:ResendCooldownSeconds " +
                "cannot be negative.");
        }
    }
}