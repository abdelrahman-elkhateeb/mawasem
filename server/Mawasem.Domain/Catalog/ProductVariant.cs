using Mawasem.Domain.Catalog;
using Mawasem.Domain.Common;

public class ProductVariant : BaseAuditableEntity
{
    public int ProductId { get; set; }

    public Product Product { get; set; } = null!;

    public string SKU { get; set; } = string.Empty;

    public int StockQuantity { get; set; }

    public bool IsAvailable { get; set; } = true;

    public ICollection<ProductVariantOption> Options { get; set; } = new List<ProductVariantOption>();

    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
}