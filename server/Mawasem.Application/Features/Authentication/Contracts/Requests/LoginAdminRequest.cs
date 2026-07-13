namespace Mawasem.Application.Features.Authentication.Contracts.Requests;

public sealed record LoginAdminRequest
{
    public string Email { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;
}