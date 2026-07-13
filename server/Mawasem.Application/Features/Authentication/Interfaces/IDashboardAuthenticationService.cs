using Mawasem.Application.Features.Authentication.Contracts.Requests;
using Mawasem.Application.Features.Authentication.Models;

namespace Mawasem.Application.Features.Authentication.Interfaces;

public interface IDashboardAuthenticationService
{
    Task<DashboardAuthenticationSessionResult> LoginAsync(
        LoginAdminRequest request ,
        string? ipAddress ,
        CancellationToken cancellationToken = default );

    Task<DashboardAuthenticationSessionResult> RefreshAsync(
        string? refreshToken ,
        string? ipAddress ,
        CancellationToken cancellationToken = default );

    Task LogoutAsync(
        string? refreshToken ,
        string? ipAddress ,
        CancellationToken cancellationToken = default );
}