using Mawasem.Domain.Common;

namespace Mawasem.Domain.Catalog;

public class ProductImage : BaseAuditableEntity
{
    public int ProductVariantId { get; set; }

    public ProductVariant ProductVariant { get; set; } = null!;

    public string ImageUrl { get; set; } = string.Empty;

    public bool IsPrimary { get; set; }

    public int DisplayOrder { get; set; }
}