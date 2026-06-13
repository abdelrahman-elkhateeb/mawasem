using Mawasem.Shared.Enums;

namespace Mawasem.Modules.Season.Application.DTOs;

public class SeasonDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public SeasonType Type { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public string? Description { get; set; }
    public string? BannerImageUrl { get; set; }
}