using Mawasem.Application.Features.Authentication.Models;

namespace Mawasem.Application.Features.Authentication.Interfaces;

public interface ICustomerUserProfileService
{
    Task<CustomerUserProfileResult> GetAsync(
        int userId ,
        CancellationToken cancellationToken = default );
}
