using Mawasem.Domain.Common;
using Mawasem.Domain.Common.ValueObjects;

namespace Mawasem.Domain.Catalog;

public class Category : BaseAuditableEntity
{
    public LocalizedText Name { get; set; } = new("" , "");

    public ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();
}