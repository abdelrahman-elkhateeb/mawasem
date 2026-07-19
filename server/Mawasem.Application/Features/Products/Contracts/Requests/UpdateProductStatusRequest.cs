namespace Mawasem.Application.Features.Products.Contracts.Requests;

public sealed record UpdateProductStatusRequest
{
    public bool IsPublished { get; init; }

    public bool IsFeatured { get; init; }
}