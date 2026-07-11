using Mawasem.Application.Features.Authentication.Models;
using Mawasem.Domain.Identity;

namespace Mawasem.Application.Features.Authentication.Interfaces;

public interface ITokenService
{
    Task<AccessTokenResult> CreateAccessTokenAsync(
        ApplicationUser user ,
        CancellationToken cancellationToken = default );

    RefreshTokenResult CreateRefreshToken();

    string HashRefreshToken( string refreshToken );
}