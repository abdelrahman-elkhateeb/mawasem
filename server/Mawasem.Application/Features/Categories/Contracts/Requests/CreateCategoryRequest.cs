namespace Mawasem.Application.Features.Categories.Contracts.Requests;

public sealed record CreateCategoryRequest
{
    public string NameAr { get; init; } = string.Empty;

    public string NameEn { get; init; } = string.Empty;
}