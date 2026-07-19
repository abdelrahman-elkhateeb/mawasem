namespace Mawasem.Application.Features.Products.Contracts.Responses;

public sealed record ProductSpecificationResponse
{
    public int Id { get; init; }

    public string NameAr { get; init; } = string.Empty;

    public string NameEn { get; init; } = string.Empty;

    public string ValueAr { get; init; } = string.Empty;

    public string ValueEn { get; init; } = string.Empty;
}