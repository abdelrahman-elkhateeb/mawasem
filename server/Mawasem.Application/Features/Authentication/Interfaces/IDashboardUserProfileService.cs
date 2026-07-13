using Mawasem.Application.Features.Authentication.Models;

namespace Mawasem.Application.Features.Authentication.Interfaces;

public interface IDashboardUserProfileService
{
    Task<DashboardUserProfileResult> GetAsync(
        int userId ,
        CancellationToken cancellationToken = default );
}