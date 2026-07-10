using Microsoft.AspNetCore.Identity;

namespace Mawasem.Domain.Identity;

public class ApplicationRole : IdentityRole<int>
{
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}