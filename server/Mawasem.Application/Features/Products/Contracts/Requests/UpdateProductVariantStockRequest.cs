namespace Mawasem.Application.Features.Products.Contracts.Requests;

public sealed record UpdateProductVariantStockRequest
{
    public int StockQuantity { get; init; }

    public string RowVersion { get; init; } = string.Empty;
}