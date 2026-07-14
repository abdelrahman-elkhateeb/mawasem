using Microsoft.AspNetCore.Authorization;

namespace Mawasem.API.Authorization;

public sealed class PermissionAuthorizationRequirement
    : IAuthorizationRequirement
{
    public PermissionAuthorizationRequirement(
        string permission )
    {
        if ( string.IsNullOrWhiteSpace(permission) )
        {
            throw new ArgumentException(
                "Permission cannot be empty." ,
                nameof(permission));
        }

        Permission = permission;
    }

    public string Permission { get; }
}