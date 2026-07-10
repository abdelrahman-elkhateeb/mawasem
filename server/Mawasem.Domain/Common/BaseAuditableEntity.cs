using Mawasem.Domain.Interfaces;

namespace Mawasem.Domain.Common

/// <summary>
/// Base entity that supports auditing and soft deletion.
/// </summary>

{
    public abstract class BaseAuditableEntity : BaseEntity, IAuditableEntity, ISoftDelete
    {
        public DateTimeOffset CreatedOn { get; set; }

        public string? CreatedBy { get; set; }

        public DateTimeOffset? LastModifiedOn { get; set; }

        public string? LastModifiedBy { get; set; }

        public bool IsDeleted { get; set; }

        public DateTimeOffset? DeletedOn { get; set; }

        public string? DeletedBy { get; set; }
    }
}
