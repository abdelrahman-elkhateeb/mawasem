namespace Mawasem.Domain.Interfaces

/// <summary>
/// Defines audit information for an entity.
/// </summary>

{
    public interface IAuditableEntity
    {
        DateTimeOffset CreatedOn { get; set; }

        string? CreatedBy { get; set; }

        DateTimeOffset? LastModifiedOn { get; set; }

        string? LastModifiedBy { get; set; }
    }
}
