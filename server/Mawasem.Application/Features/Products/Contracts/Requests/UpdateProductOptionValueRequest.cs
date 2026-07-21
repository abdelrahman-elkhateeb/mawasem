namespace Mawasem.Application.Features.Products.Contracts.Requests;

public sealed record UpdateProductOptionValueRequest
{
    public string ValueAr { get; init; } = string.Empty;

    public string ValueEn { get; init; } = string.Empty;
}