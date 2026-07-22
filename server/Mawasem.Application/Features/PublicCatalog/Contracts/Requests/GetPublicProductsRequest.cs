using Mawasem.Application.Features.PublicCatalog.Models;
using System.ComponentModel.DataAnnotations;

namespace Mawasem.Application.Features.PublicCatalog.Contracts.Requests;

public sealed class GetPublicProductsRequest : IValidatableObject
{
    public string? SearchTerm { get; set; }

    public int? SeasonId { get; set; }

    public int? CollectionId { get; set; }

    public int? CategoryId { get; set; }

    public int? BrandId { get; set; }

    public int? GradeId { get; set; }

    public int? TagId { get; set; }

    public decimal? MinimumPrice { get; set; }

    public decimal? MaximumPrice { get; set; }

    public bool InStockOnly { get; set; }

    public bool? IsFeatured { get; set; }

    public PublicProductSortOption SortBy { get; set; } =
        PublicProductSortOption.Newest;

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext )
    {
        if ( SearchTerm is { Length: > 200 } )
        {
            yield return new ValidationResult(
                "Search term cannot exceed 200 characters." ,
                [nameof(SearchTerm)]);
        }

        var identifierFilters = new (int? Value , string PropertyName)[]
        {
            (SeasonId, nameof(SeasonId)),
            (CollectionId, nameof(CollectionId)),
            (CategoryId, nameof(CategoryId)),
            (BrandId, nameof(BrandId)),
            (GradeId, nameof(GradeId)),
            (TagId, nameof(TagId))
        };

        foreach ( var filter in identifierFilters )
        {
            if ( filter.Value.HasValue && filter.Value.Value <= 0 )
            {
                yield return new ValidationResult(
                    $"{filter.PropertyName} must be greater than zero." ,
                    [filter.PropertyName]);
            }
        }

        if ( MinimumPrice.HasValue && MinimumPrice.Value < 0 )
        {
            yield return new ValidationResult(
                "Minimum price cannot be negative." ,
                [nameof(MinimumPrice)]);
        }

        if ( MaximumPrice.HasValue && MaximumPrice.Value < 0 )
        {
            yield return new ValidationResult(
                "Maximum price cannot be negative." ,
                [nameof(MaximumPrice)]);
        }

        if ( MinimumPrice.HasValue &&
            MaximumPrice.HasValue &&
            MinimumPrice.Value > MaximumPrice.Value )
        {
            yield return new ValidationResult(
                "Minimum price cannot be greater than maximum price." ,
                [nameof(MinimumPrice) , nameof(MaximumPrice)]);
        }

        if ( !Enum.IsDefined(typeof(PublicProductSortOption) , SortBy) )
        {
            yield return new ValidationResult(
                "The selected sort option is invalid." ,
                [nameof(SortBy)]);
        }

        if ( PageNumber < 1 )
        {
            yield return new ValidationResult(
                "Page number must be at least 1." ,
                [nameof(PageNumber)]);
        }

        if ( PageSize is < 1 or > 100 )
        {
            yield return new ValidationResult(
                "Page size must be between 1 and 100." ,
                [nameof(PageSize)]);
        }
    }
}