using Mawasem.Shared.Abstractions;
using Mawasem.Shared.Enums;

namespace Mawasem.Modules.Season.Domain.Entities;

public class SeasonEntity : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public SeasonType Type { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public bool IsActive { get; private set; }
    public string? Description { get; private set; }
    public string? BannerImageUrl { get; private set; }

    private SeasonEntity() { }

    public static SeasonEntity Create(
        string name ,
        SeasonType type ,
        DateTime startDate ,
        DateTime endDate ,
        string? description = null ,
        string? bannerImageUrl = null )
    {
        if ( string.IsNullOrWhiteSpace(name) )
            throw new ArgumentException("Season name cannot be empty");

        if ( endDate <= startDate )
            throw new ArgumentException("End date must be after start date");

        return new SeasonEntity
        {
            Name = name ,
            Type = type ,
            StartDate = startDate ,
            EndDate = endDate ,
            IsActive = false ,
            Description = description ,
            BannerImageUrl = bannerImageUrl
        };
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(
        string name ,
        DateTime startDate ,
        DateTime endDate ,
        string? description = null ,
        string? bannerImageUrl = null )
    {
        if ( string.IsNullOrWhiteSpace(name) )
            throw new ArgumentException("Season name cannot be empty");

        if ( endDate <= startDate )
            throw new ArgumentException("End date must be after start date");

        Name = name;
        StartDate = startDate;
        EndDate = endDate;
        Description = description;
        BannerImageUrl = bannerImageUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsCurrentlyActive()
    {
        var now = DateTime.UtcNow;
        return IsActive && StartDate <= now && EndDate >= now;
    }
}