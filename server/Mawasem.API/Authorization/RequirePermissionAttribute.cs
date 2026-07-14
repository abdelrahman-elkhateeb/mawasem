using Mawasem.Domain.Identity;
using Microsoft.AspNetCore.Authorization;

namespace Mawasem.API.Authorization;

[AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Method ,
    AllowMultiple = true ,
    Inherited = true)]
public sealed class RequirePermissionAttribute
    : AuthorizeAttribute
{
    public RequirePermissionAttribute(
        string permission )
    {
        if ( string.IsNullOrWhiteSpace(permission) )
        {
            throw new ArgumentException(
                "Permission cannot be empty." ,
                nameof(permission));
        }

        if ( !SystemPermissions.All.Contains(
                permission ,
                StringComparer.Ordinal) )
        {
            throw new ArgumentOutOfRangeException(
                nameof(permission) ,
                permission ,
                "The specified system permission is not registered.");
        }

        Policy = permission;
    }
}