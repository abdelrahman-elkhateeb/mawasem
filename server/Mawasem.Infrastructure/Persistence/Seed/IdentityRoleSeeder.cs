using Mawasem.Domain.Identity;
using Microsoft.AspNetCore.Identity;

namespace Mawasem.Infrastructure.Persistence.Seed;

public sealed class IdentityRoleSeeder
{
    private readonly RoleManager<ApplicationRole> _roleManager;

    public IdentityRoleSeeder(
        RoleManager<ApplicationRole> roleManager )
    {
        _roleManager = roleManager;
    }

    public async Task SeedAsync()
    {
        foreach ( var roleName in SystemRoles.All )
        {
            var roleExists =
                await _roleManager.RoleExistsAsync(roleName);

            if ( roleExists )
            {
                continue;
            }

            var result =
                await _roleManager.CreateAsync(
                    new ApplicationRole
                    {
                        Name = roleName
                    });

            if ( result.Succeeded )
            {
                continue;
            }

            var errors = string.Join(
                "; " ,
                result.Errors.Select(
                    error => error.Description));

            throw new InvalidOperationException(
                $"Failed to create the role '{roleName}': {errors}");
        }
    }
}