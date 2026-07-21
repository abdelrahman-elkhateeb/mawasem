namespace Mawasem.Application.Features.Products.Contracts.Requests;

public sealed record CreateProductOptionRequest
{
    public string NameAr { get; init; } = string.Empty;

    public string NameEn { get; init; } = string.Empty;
}