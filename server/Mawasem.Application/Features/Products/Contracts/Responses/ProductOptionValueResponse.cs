namespace Mawasem.Application.Features.Products.Contracts.Responses;

public sealed record ProductOptionValueResponse
{
    public int Id { get; init; }

    public string ValueAr { get; init; } = string.Empty;

    public string ValueEn { get; init; } = string.Empty;
}