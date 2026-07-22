namespace Mawasem.Application.Features.Carts.Models;

public static class CartWarningCodes
{
    public const string ProductUnavailable =
        "carts.warning.product_unavailable";

    public const string ProductVariantUnavailable =
        "carts.warning.product_variant_unavailable";

    public const string OutOfStock =
        "carts.warning.out_of_stock";

    public const string InsufficientStock =
        "carts.warning.insufficient_stock";

    public const string PriceChanged =
        "carts.warning.price_changed";
}
