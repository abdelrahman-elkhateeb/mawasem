namespace Mawasem.Application.Features.Authentication.Interfaces;

public interface ICustomerPasswordResetCodeSender
{
    Task SendAsync(
        string normalizedPhoneNumber ,
        string code ,
        DateTime expiresAtUtc ,
        CancellationToken cancellationToken = default );
}