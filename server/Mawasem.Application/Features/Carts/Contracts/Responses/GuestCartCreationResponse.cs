namespace Mawasem.Application.Features.Carts.Contracts.Responses;

public sealed record GuestCartCreationResponse
{
    public int Id { get; init; }

    public string Token { get; init; } = string.Empty;

    public DateTimeOffset ExpiresOn { get; init; }
}
