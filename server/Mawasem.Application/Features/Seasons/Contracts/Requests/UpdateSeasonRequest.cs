namespace Mawasem.Application.Features.Seasons.Contracts.Requests;

public sealed record UpdateSeasonRequest
{
    public string NameAr { get; init; } = string.Empty;

    public string NameEn { get; init; } = string.Empty;

    public string DescriptionAr { get; init; } = string.Empty;

    public string DescriptionEn { get; init; } = string.Empty;

    public bool IsActive { get; init; }
}