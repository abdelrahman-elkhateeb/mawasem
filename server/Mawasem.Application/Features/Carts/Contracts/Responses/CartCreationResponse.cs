namespace Mawasem.Application.Features.Carts.Contracts.Responses;

public sealed record CartCreationResponse
{
    public int Id { get; init; }

    public bool WasCreated { get; init; }
}
