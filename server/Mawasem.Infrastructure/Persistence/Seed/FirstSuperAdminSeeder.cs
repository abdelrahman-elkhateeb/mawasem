using Mawasem.Application.Features.Authentication.Options;
using Mawasem.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace Mawasem.Infrastructure.Persistence.Seed;

public sealed class FirstSuperAdminSeeder
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly SuperAdminSeedOptions _options;

    public FirstSuperAdminSeeder(
        UserManager<ApplicationUser> userManager ,
        RoleManager<ApplicationRole> roleManager ,
        IOptions<SuperAdminSeedOptions> options )
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _options = options.Value;
    }

    public async Task SeedAsync()
    {
        var existingSuperAdmins =
            await _userManager.GetUsersInRoleAsync(
                SystemRoles.SuperAdmin);

        // The seed is strictly for creating the first SuperAdmin.
        // It never modifies or replaces an existing SuperAdmin.
        if ( existingSuperAdmins.Count > 0 )
        {
            return;
        }

        // Allow the application to run before seed secrets are configured.
        if ( !_options.HasAnyValue() )
        {
            return;
        }

        if ( !_options.IsComplete() )
        {
            throw new InvalidOperationException(
                "The first SuperAdmin seed configuration is incomplete. " +
                "AdminSeed:Email, AdminSeed:Password, " +
                "AdminSeed:FullNameAr, and AdminSeed:FullNameEn " +
                "must all be provided.");
        }

        var email =
            _options.Email.Trim();

        if ( !new EmailAddressAttribute().IsValid(email) )
        {
            throw new InvalidOperationException(
                "AdminSeed:Email is not a valid email address.");
        }

        var fullNameAr =
            _options.FullNameAr.Trim();

        var fullNameEn =
            _options.FullNameEn.Trim();

        if ( fullNameAr.Length > 200 ||
            fullNameEn.Length > 200 )
        {
            throw new InvalidOperationException(
                "The seeded SuperAdmin names cannot exceed 200 characters.");
        }

        var superAdminRoleExists =
            await _roleManager.RoleExistsAsync(
                SystemRoles.SuperAdmin);

        if ( !superAdminRoleExists )
        {
            throw new InvalidOperationException(
                "The SuperAdmin role was not found. " +
                "Run the role seeder before the first SuperAdmin seeder.");
        }

        var existingUser =
            await _userManager.FindByEmailAsync(email);

        if ( existingUser is not null )
        {
            throw new InvalidOperationException(
                $"A user with the email '{email}' already exists, " +
                "but no SuperAdmin account currently exists.");
        }

        var superAdmin =
            new ApplicationUser
            {
                UserName = email ,
                Email = email ,
                EmailConfirmed = true ,

                FullNameAr = fullNameAr ,
                FullNameEn = fullNameEn ,

                PhoneNumber = null ,
                PhoneNumberConfirmed = false ,

                BirthDate = null ,
                Gender = null ,
                ReferralSource = null ,

                IsBlocked = false ,
                BlockedAt = null ,
                BlockedReason = null ,

                MustChangePassword = false ,
                LockoutEnabled = true
            };

        var createResult =
            await _userManager.CreateAsync(
                superAdmin ,
                _options.Password);

        if ( !createResult.Succeeded )
        {
            throw CreateException(
                "Failed to create the first SuperAdmin" ,
                createResult.Errors);
        }

        var roleResult =
            await _userManager.AddToRoleAsync(
                superAdmin ,
                SystemRoles.SuperAdmin);

        if ( roleResult.Succeeded )
        {
            return;
        }

        // Avoid leaving an account without its required SuperAdmin role.
        var deleteResult =
            await _userManager.DeleteAsync(superAdmin);

        var roleErrors =
            string.Join(
                "; " ,
                roleResult.Errors.Select(
                    error => error.Description));

        var cleanupErrors =
            deleteResult.Succeeded
                ? string.Empty
                : string.Join(
                    "; " ,
                    deleteResult.Errors.Select(
                        error => error.Description));

        var message =
            $"Failed to assign the SuperAdmin role: {roleErrors}";

        if ( !string.IsNullOrWhiteSpace(cleanupErrors) )
        {
            message +=
                $". Cleanup also failed: {cleanupErrors}";
        }

        throw new InvalidOperationException(message);
    }

    private static InvalidOperationException CreateException(
        string message ,
        IEnumerable<IdentityError> errors )
    {
        var errorMessage =
            string.Join(
                "; " ,
                errors.Select(
                    error => error.Description));

        return new InvalidOperationException(
            $"{message}: {errorMessage}");
    }
}