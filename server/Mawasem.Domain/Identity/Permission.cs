using Mawasem.Domain.Common;

namespace Mawasem.Domain.Identity;

public class Permission : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public ICollection<RolePermission> RolePermissions { get; set; } =
        new List<RolePermission>();

    public ICollection<UserPermission> UserPermissions { get; set; } =
        new List<UserPermission>();
}