using Mawasem.Domain.Catalog;
using Mawasem.Domain.Common;
using Mawasem.Domain.Common.ValueObjects;
using Mawasem.Domain.Reviews;

public class Product : BaseAuditableEntity
{
    // Localization
    public LocalizedText Name { get; set; } = new("" , "");
    public LocalizedText Description { get; set; } = new("" , "");

    // Pricing
    public decimal OriginalPrice { get; set; }
    public decimal CurrentPrice { get; set; }

    // Status
    public bool IsPublished { get; set; } = true;
    public bool IsFeatured { get; set; }

    // SEO
    public string Slug { get; set; } = string.Empty;

    // Relations
    public int BrandId { get; set; }
    public Brand Brand { get; set; } = null!;

    public int SeasonId { get; set; }
    public Season Season { get; set; } = null!;

    // Navigation
    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();

    public ICollection<ProductSpecification> Specifications { get; set; } = new List<ProductSpecification>();

    public ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();

    public ICollection<ProductCollection> ProductCollections { get; set; } = new List<ProductCollection>();

    public ICollection<ProductGrade> ProductGrades { get; set; } = new List<ProductGrade>();

    public ICollection<ProductTag> ProductTags { get; set; } = new List<ProductTag>();

    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}