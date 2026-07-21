using Mawasem.Domain.Enums;

namespace Mawasem.Application.Features.Products.Contracts.Responses;

public sealed record ProductOptionResponse
{
    public int Id { get; init; }

    public string NameAr { get; init; } = string.Empty;

    public string NameEn { get; init; } = string.Empty;

    public ProductOptionType Type { get; init; }

    public IReadOnlyCollection<ProductOptionValueResponse> Values { get; init; } =
        Array.Empty<ProductOptionValueResponse>();
}