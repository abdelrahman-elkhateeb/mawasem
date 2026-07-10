namespace Mawasem.Domain.Interfaces

/// <summary>
/// Defines soft delete behavior.
/// </summary>

{
    public interface ISoftDelete
    {
        bool IsDeleted { get; set; }

        DateTimeOffset? DeletedOn { get; set; }

        string? DeletedBy { get; set; }
    }
}
