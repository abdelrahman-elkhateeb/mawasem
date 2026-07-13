namespace Mawasem.Application.Features.Authentication.Contracts.Requests;

public sealed record LoginCustomerRequest
{
    public string PhoneNumber { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;
}