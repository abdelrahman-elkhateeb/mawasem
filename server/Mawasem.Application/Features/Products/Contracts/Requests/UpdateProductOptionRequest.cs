namespace Mawasem.Application.Features.Products.Contracts.Requests;

public sealed record UpdateProductOptionRequest
{
    public string NameAr { get; init; } = string.Empty;

    public string NameEn { get; init; } = string.Empty;
}