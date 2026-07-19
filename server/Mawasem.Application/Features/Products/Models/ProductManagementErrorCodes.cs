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
}