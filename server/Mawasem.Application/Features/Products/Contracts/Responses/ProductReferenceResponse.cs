namespace Mawasem.Application.Features.Products.Contracts.Responses;

public sealed record ProductReferenceResponse
{
    public int Id { get; init; }

    public string NameAr { get; init; } = string.Empty;

    public string NameEn { get; init; } = string.Empty;
}