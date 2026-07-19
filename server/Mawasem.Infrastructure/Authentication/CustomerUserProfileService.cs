using Mawasem.Application.Features.Authentication.Contracts.Responses;
using Mawasem.Application.Features.Authentication.Interfaces;
using Mawasem.Application.Features.Authentication.Models;
using Mawasem.Domain.Identity;
using Mawasem.Infrastructure.Persistence.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Mawasem.Infrastructure.Authentication;

public sealed class CustomerUserProfileService
    : ICustomerUserProfileService
{
    private readonly UserManager<ApplicationUser> _userManager;

    private readonly MawasemDbContext _dbContext;

    public CustomerUserProfileService(
        UserManager<ApplicationUser> userManager ,
        MawasemDbContext dbContext )
    {
        _userManager = userManager;
        _dbContext = dbContext;
    }

    public async Task<CustomerUserProfileResult> GetAsync(
        int userId ,
        CancellationToken cancellationToken = default )
    {
        if ( userId <= 0 )
        {
            return CustomerUserProfileResult.Failure(
                AuthenticationErrorCodes.InvalidCredentials ,
                "The authenticated customer account is invalid.");
        }

        var user =
            await _dbContext.Users
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    applicationUser =>
                        applicationUser.Id == userId ,
                    cancellationToken);

        if ( user is null )
        {
            return CustomerUserProfileResult.Failure(
                AuthenticationErrorCodes.InvalidCredentials ,
                "The authenticated customer account was not found.");
        }

        if ( user.IsBlocked )
        {
            return CustomerUserProfileResult.Failure(
                AuthenticationErrorCodes.AccountBlocked ,
                "This customer account has been blocked.");
        }

        var assignedRoles =
            await _userManager.GetRolesAsync(user);

        var customerRoles =
            assignedRoles
                .Where(roleName =>
                    string.Equals(
                        roleName ,
                        SystemRoles.Customer ,
                        StringComparison.OrdinalIgnoreCase))
                .OrderBy(roleName => roleName)
                .ToArray();

        if ( customerRoles.Length == 0 )
        {
            return CustomerUserProfileResult.Failure(
                AuthenticationErrorCodes.InvalidCredentials ,
                "The authenticated account is not a customer account.");
        }

        var response =
            new AuthenticatedUserResponse
            {
                Id = user.Id ,
                FullNameAr = user.FullNameAr ,
                FullNameEn = user.FullNameEn ,
                PhoneNumber = user.PhoneNumber ,
                Email = user.Email ,
                Roles = customerRoles
            };

        return CustomerUserProfileResult.Success(response);
    }
}