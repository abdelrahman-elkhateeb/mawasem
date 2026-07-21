namespace Mawasem.Application.Features.Products.Models;

public static class ProductManagementErrorCodes
{
    public const string InvalidRequest =
        "products.invalid_request";

    public const string NotFound =
        "products.not_found";

    public const string DuplicateSlug =
        "products.duplicate_slug";

    public const string InvalidReference =
        "products.invalid_reference";

    public const string CannotPublish =
        "products.cannot_publish";

    public const string InvalidImage =
        "products.invalid_image";

    public const string ImageNotFound =
        "products.image_not_found";

    public const string InvalidImageOrder =
        "products.invalid_image_order";
}