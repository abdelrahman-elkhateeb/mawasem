using Mawasem.Domain.Common;
using Mawasem.Domain.Common.ValueObjects;

namespace Mawasem.Domain.Catalog;

public class ProductSpecification : BaseAuditableEntity
{
    public int ProductId { get; set; }

    public Product Product { get; set; } = null!;

    public LocalizedText Name { get; set; } = new("" , "");

    public LocalizedText Value { get; set; } = new("" , "");
}