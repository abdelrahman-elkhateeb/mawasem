using Mawasem.Domain.Common;
using Mawasem.Domain.Common.ValueObjects;

namespace Mawasem.Domain.Catalog;

public class Tag : BaseAuditableEntity
{
    public LocalizedText Name { get; set; } = new("" , "");

    public ICollection<ProductTag> ProductTags { get; set; } = new List<ProductTag>();
}