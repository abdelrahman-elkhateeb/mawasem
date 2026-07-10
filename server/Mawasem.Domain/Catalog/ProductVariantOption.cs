using Mawasem.Domain.Common;

namespace Mawasem.Domain.Catalog;

public class ProductVariantOption : BaseAuditableEntity
{
    public int ProductVariantId { get; set; }

    public ProductVariant ProductVariant { get; set; } = null!;

    public int ProductOptionValueId { get; set; }

    public ProductOptionValue ProductOptionValue { get; set; } = null!;
}