namespace Mawasem.Application.Features.Carts.Contracts.Responses;

public sealed record CartDetailsResponse
{
    public int CartId { get; init; }

    public bool IsGuest { get; init; }

    public DateTimeOffset? GuestExpiresOn { get; init; }

    public int DistinctItemCount { get; init; }

    public int TotalQuantity { get; init; }

    public decimal Subtotal { get; init; }

    public bool HasWarnings { get; init; }

    public IReadOnlyCollection<CartItemResponse> Items { get; init; } =
        Array.Empty<CartItemResponse>();
}

public sealed record CartItemResponse
{
    public int CartItemId { get; init; }

    public int ProductVariantId { get; init; }

    public int ProductId { get; init; }

    public string ProductNameEn { get; init; } = string.Empty;

    public string ProductNameAr { get; init; } = string.Empty;

    public string Sku { get; init; } = string.Empty;

    public string OptionCombinationKey { get; init; } = string.Empty;

    public int Quantity { get; init; }

    public decimal UnitPriceSnapshot { get; init; }

    public decimal CurrentUnitPrice { get; init; }

    public decimal LineTotal { get; init; }

    public int StockQuantity { get; init; }

    public bool IsPurchasable { get; init; }

    public IReadOnlyCollection<CartWarningResponse> Warnings { get; init; } =
        Array.Empty<CartWarningResponse>();
}

public sealed record CartWarningResponse
{
    public string Code { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;
}
