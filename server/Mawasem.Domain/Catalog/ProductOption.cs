using Mawasem.Domain.Common;
using Mawasem.Domain.Common.ValueObjects;
using Mawasem.Domain.Enums;

namespace Mawasem.Domain.Catalog;

public class ProductOption : BaseAuditableEntity
{
    public LocalizedText Name { get; set; } = new("" , "");

    public ProductOptionType Type { get; set; } =
        ProductOptionType.Standard;

    public ICollection<ProductOptionValue> Values { get; set; } =
        new List<ProductOptionValue>();
}