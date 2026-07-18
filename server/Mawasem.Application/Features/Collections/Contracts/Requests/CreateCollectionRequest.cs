namespace Mawasem.Application.Features.Collections.Contracts.Requests;

public sealed record CreateCollectionRequest
{
    public string NameAr { get; init; } = string.Empty;

    public string NameEn { get; init; } = string.Empty;
}