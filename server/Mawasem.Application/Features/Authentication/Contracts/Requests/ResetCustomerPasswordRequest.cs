namespace Mawasem.Application.Features.Authentication.Contracts.Requests;

public sealed record ResetCustomerPasswordRequest
{
    public string PhoneNumber { get; init; } = string.Empty;

    public string ResetToken { get; init; } = string.Empty;

    public string NewPassword { get; init; } = string.Empty;

    public string ConfirmNewPassword { get; init; } = string.Empty;
}