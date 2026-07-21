namespace Mawasem.Application.Features.Products.Contracts.Requests;

public sealed record CreateProductVariantRequest
{
    public IReadOnlyCollection<int> OptionValueIds { get; init; } =
        Array.Empty<int>();
}