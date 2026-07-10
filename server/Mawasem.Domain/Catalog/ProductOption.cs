using Mawasem.Domain.Common;
using Mawasem.Domain.Common.ValueObjects;

namespace Mawasem.Domain.Catalog;

public class ProductOption : BaseAuditableEntity
{
    public LocalizedText Name { get; set; } = new("" , "");

    public ICollection<ProductOptionValue> Values { get; set; } = new List<ProductOptionValue>();
}