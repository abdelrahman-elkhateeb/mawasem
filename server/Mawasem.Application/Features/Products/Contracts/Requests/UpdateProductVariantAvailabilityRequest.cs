namespace Mawasem.Application.Features.Products.Contracts.Requests;

public sealed record UpdateProductVariantAvailabilityRequest
{
    public bool IsAvailable { get; init; }
}