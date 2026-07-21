namespace Mawasem.Application.Features.Products.Contracts.Responses;

public sealed record ProductVariantOptionResponse
{
    public int OptionId { get; init; }

    public string OptionNameAr { get; init; } = string.Empty;

    public string OptionNameEn { get; init; } = string.Empty;

    public int ValueId { get; init; }

    public string ValueAr { get; init; } = string.Empty;

    public string ValueEn { get; init; } = string.Empty;
}