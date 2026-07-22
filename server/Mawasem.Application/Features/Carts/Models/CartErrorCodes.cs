namespace Mawasem.Application.Features.Carts.Models;

public static class CartErrorCodes
{
    public const string InvalidCustomer =
        "carts.invalid_customer";

    public const string AccountBlocked =
        "carts.account_blocked";

    public const string InvalidGuestToken =
        "carts.invalid_guest_token";

    public const string GuestCartNotFound =
        "carts.guest_cart_not_found";

    public const string GuestCartExpired =
        "carts.guest_cart_expired";

    public const string InvalidProductVariant =
        "carts.invalid_product_variant";

    public const string ProductVariantNotFound =
        "carts.product_variant_not_found";

    public const string ProductUnavailable =
        "carts.product_unavailable";

    public const string ProductVariantUnavailable =
        "carts.product_variant_unavailable";

    public const string InvalidQuantity =
        "carts.invalid_quantity";

    public const string InsufficientStock =
        "carts.insufficient_stock";

    public const string CartItemNotFound =
        "carts.cart_item_not_found";
}
