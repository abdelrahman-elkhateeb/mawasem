namespace Mawasem.Application.Features.Products.Models;

public static class ProductVariantManagementErrorCodes
{
    public const string ProductNotFound =
        "ProductNotFound";

    public const string VariantNotFound =
        "ProductVariantNotFound";

    public const string OptionValueNotFound =
        "ProductOptionValueNotFound";

    public const string DuplicateOptionValueSelection =
        "DuplicateProductOptionValueSelection";

    public const string MultipleValuesForSameOption =
        "MultipleValuesForSameProductOption";

    public const string InconsistentOptionStructure =
        "InconsistentProductOptionStructure";

    public const string CombinationAlreadyExists =
        "VariantCombinationAlreadyExists";

    public const string StockQuantityCannotBeNegative =
        "StockQuantityCannotBeNegative";

    public const string InvalidRowVersion =
        "InvalidRowVersion";

    public const string StockConcurrencyConflict =
        "StockConcurrencyConflict";
}