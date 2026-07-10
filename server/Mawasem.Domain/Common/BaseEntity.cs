namespace Mawasem.Domain.Common

/// <summary>
/// Represents the base entity for all domain entities.
/// </summary>

{
    public abstract class BaseEntity
    {
        public int Id { get; set; }
    }
}