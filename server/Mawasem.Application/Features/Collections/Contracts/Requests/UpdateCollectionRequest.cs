namespace Mawasem.Application.Features.Collections.Contracts.Requests;

public sealed class UpdateCollectionRequest
{
    public string NameEn { get; init; } = string.Empty;

    public string NameAr { get; init; } = string.Empty;

    public int SeasonId { get; init; }
}