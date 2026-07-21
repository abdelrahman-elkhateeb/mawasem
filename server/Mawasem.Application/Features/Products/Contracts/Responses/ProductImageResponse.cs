namespace Mawasem.Application.Features.Products.Contracts.Responses;

public sealed record ProductImageResponse
{
    public int Id { get; init; }

    public int ProductId { get; init; }

    public int? ColorOptionValueId { get; init; }

    public string? ColorValueAr { get; init; }

    public string? ColorValueEn { get; init; }

    public string ImageUrl { get; init; } =
        string.Empty;

    public bool IsPrimary { get; init; }

    public int DisplayOrder { get; init; }

    public DateTimeOffset CreatedOn { get; init; }
}