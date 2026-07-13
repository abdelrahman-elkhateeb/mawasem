using Mawasem.Application.Features.Authentication.Interfaces;
using Microsoft.Extensions.Logging;

namespace Mawasem.Infrastructure.Authentication;

public sealed class DevelopmentCustomerPasswordResetCodeSender
    : ICustomerPasswordResetCodeSender
{
    private readonly ILogger<DevelopmentCustomerPasswordResetCodeSender>
        _logger;

    public DevelopmentCustomerPasswordResetCodeSender(
        ILogger<DevelopmentCustomerPasswordResetCodeSender> logger )
    {
        _logger = logger;
    }

    public Task SendAsync(
        string normalizedPhoneNumber ,
        string code ,
        DateTime expiresAtUtc ,
        CancellationToken cancellationToken = default )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var maskedPhoneNumber =
            MaskPhoneNumber(normalizedPhoneNumber);

        _logger.LogWarning(
            "DEVELOPMENT ONLY - Customer password reset code " +
            "for {MaskedPhoneNumber}: {ResetCode}. " +
            "Expires at {ExpiresAtUtc}." ,
            maskedPhoneNumber ,
            code ,
            expiresAtUtc);

        return Task.CompletedTask;
    }

    private static string MaskPhoneNumber(
        string normalizedPhoneNumber )
    {
        if ( string.IsNullOrWhiteSpace(normalizedPhoneNumber) )
        {
            return "unknown";
        }

        const int visibleDigits = 4;

        if ( normalizedPhoneNumber.Length <= visibleDigits )
        {
            return normalizedPhoneNumber;
        }

        return
            new string(
                '*' ,
                normalizedPhoneNumber.Length - visibleDigits)
            + normalizedPhoneNumber[^visibleDigits..];
    }
}