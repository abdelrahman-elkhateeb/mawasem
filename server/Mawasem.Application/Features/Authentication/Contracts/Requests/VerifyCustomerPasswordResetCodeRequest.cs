namespace Mawasem.Application.Features.Authentication.Contracts.Requests;

public sealed record VerifyCustomerPasswordResetCodeRequest
{
    public string PhoneNumber { get; init; } = string.Empty;

    public string Code { get; init; } = string.Empty;
}