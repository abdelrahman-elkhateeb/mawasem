namespace Mawasem.Application.Features.Authentication.Contracts.Requests;

public sealed record ForgotCustomerPasswordRequest
{
    public string PhoneNumber { get; init; } = string.Empty;
}