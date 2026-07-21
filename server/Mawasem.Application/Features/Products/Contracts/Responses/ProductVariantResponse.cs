namespace Mawasem.Application.Features.Products.Contracts.Responses;

public sealed record ProductVariantResponse
{
    public int Id { get; init; }

    public int ProductId { get; init; }

    public string SKU { get; init; } = string.Empty;

    public int StockQuantity { get; init; }

    public bool IsAvailable { get; init; }

    public bool CanPurchase { get; init; }

    public string RowVersion { get; init; } = string.Empty;

    public IReadOnlyCollection<ProductVariantOptionResponse> Options { get; init; } =
        Array.Empty<ProductVariantOptionResponse>();
}