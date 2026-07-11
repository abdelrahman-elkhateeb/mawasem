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
        var customerRoleExists =
            await _roleManager.RoleExistsAsync(
                SystemRoles.Customer);

        if ( customerRoleExists )
        {
            return;
        }

        var result =
            await _roleManager.CreateAsync(
                new ApplicationRole
                {
                    Name = SystemRoles.Customer
                });

        if ( result.Succeeded )
        {
            return;
        }

        var errors = string.Join(
            "; " ,
            result.Errors.Select(x => x.Description));

        throw new InvalidOperationException(
            $"Failed to create the Customer role: {errors}");
    }
}