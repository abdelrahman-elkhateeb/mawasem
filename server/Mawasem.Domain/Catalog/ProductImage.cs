using Mawasem.Domain.Common;

namespace Mawasem.Domain.Catalog;

public class ProductImage : BaseAuditableEntity
{
    public int ProductId { get; set; }

    public Product Product { get; set; } = null!;

    public int? ColorOptionValueId { get; set; }

    public ProductOptionValue? ColorOptionValue { get; set; }

    public string ImageUrl { get; set; } = string.Empty;

    public string StorageKey { get; set; } = string.Empty;

    public bool IsPrimary { get; set; }

    public int DisplayOrder { get; set; }
}