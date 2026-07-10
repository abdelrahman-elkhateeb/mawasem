using Mawasem.Domain.Common;
using Mawasem.Domain.Common.ValueObjects;

namespace Mawasem.Domain.Catalog;

public class Brand : BaseAuditableEntity
{
    public LocalizedText Name { get; set; } = new("" , "");

    public LocalizedText Description { get; set; } = new("" , "");

    public string? LogoUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Product> Products { get; set; } = new List<Product>();
}