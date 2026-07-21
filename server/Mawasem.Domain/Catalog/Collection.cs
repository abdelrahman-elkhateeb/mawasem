using Mawasem.Domain.Common;
using Mawasem.Domain.Common.ValueObjects;

namespace Mawasem.Domain.Catalog;

public class Collection : BaseAuditableEntity
{
    public LocalizedText Name { get; set; } = new("" , "");

    public int SeasonId { get; set; }

    public Season Season { get; set; } = null!;

    public ICollection<ProductCollection> ProductCollections { get; set; } =
        new List<ProductCollection>();
}