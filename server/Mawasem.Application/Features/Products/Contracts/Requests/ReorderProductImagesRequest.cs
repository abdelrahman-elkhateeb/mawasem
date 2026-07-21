namespace Mawasem.Application.Features.Products.Contracts.Requests;

public sealed record ReorderProductImagesRequest
{
    public int? ColorOptionValueId { get; init; }

    public IReadOnlyCollection<int> ImageIds { get; init; } =
        Array.Empty<int>();
}