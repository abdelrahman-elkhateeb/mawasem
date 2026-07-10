using Mawasem.Domain.Common;
using Mawasem.Domain.Common.ValueObjects;

namespace Mawasem.Domain.Catalog;

public class ProductOptionValue : BaseAuditableEntity
{
    public int ProductOptionId { get; set; }

    public ProductOption ProductOption { get; set; } = null!;

    public LocalizedText Value { get; set; } = new("" , "");

    public ICollection<ProductVariantOption> ProductVariantOptions { get; set; } = new List<ProductVariantOption>();
}