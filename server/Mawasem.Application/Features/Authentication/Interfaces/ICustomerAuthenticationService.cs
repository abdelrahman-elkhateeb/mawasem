using Mawasem.Application.Features.Authentication.Contracts.Requests;
using Mawasem.Application.Features.Authentication.Models;

namespace Mawasem.Application.Features.Authentication.Interfaces;

public interface ICustomerAuthenticationService
{
    Task<AuthenticationSessionResult> RegisterAsync(
        RegisterCustomerRequest request ,
        string? ipAddress ,
        CancellationToken cancellationToken = default );

    Task<AuthenticationSessionResult> LoginAsync(
        LoginCustomerRequest request ,
        string? ipAddress ,
        CancellationToken cancellationToken = default );
}