using Mawasem.Application.Features.Authentication.Interfaces;
using Mawasem.Application.Features.Authentication.Models;
using Mawasem.Application.Features.Authentication.Options;
using Mawasem.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Mawasem.Infrastructure.Authentication;

public sealed class JwtTokenService : ITokenService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly JwtSettings _jwtSettings;
    private readonly TimeProvider _timeProvider;
    private readonly JsonWebTokenHandler _tokenHandler;
    private readonly SigningCredentials _signingCredentials;

    public JwtTokenService(
        UserManager<ApplicationUser> userManager ,
        IOptions<JwtSettings> jwtOptions ,
        TimeProvider timeProvider )
    {
        _userManager = userManager;
        _jwtSettings = jwtOptions.Value;
        _timeProvider = timeProvider;

        byte[] keyBytes;

        try
        {
            keyBytes = Convert.FromBase64String(_jwtSettings.Key);
        }
        catch ( FormatException exception )
        {
            throw new InvalidOperationException(
                "The JWT signing key must be a valid Base64 value." ,
                exception);
        }

        if ( keyBytes.Length < 32 )
        {
            throw new InvalidOperationException(
                "The JWT signing key must contain at least 32 bytes.");
        }

        _signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(keyBytes) ,
            SecurityAlgorithms.HmacSha256);

        _tokenHandler = new JsonWebTokenHandler();
    }

    public async Task<AccessTokenResult> CreateAccessTokenAsync(
        ApplicationUser user ,
        CancellationToken cancellationToken = default )
    {
        ArgumentNullException.ThrowIfNull(user);

        cancellationToken.ThrowIfCancellationRequested();

        var now = _timeProvider.GetUtcNow().UtcDateTime;

        var expiresAtUtc = now.AddMinutes(
            _jwtSettings.AccessTokenMinutes);

        var userId = user.Id.ToString(
            CultureInfo.InvariantCulture);

        var displayName = GetDisplayName(user , userId);

        var claims = new List<Claim>
        {
            new(
                JwtClaimNames.Subject ,
                userId),

            new(
                JwtClaimNames.TokenId ,
                Guid.NewGuid().ToString("N")),

            new(
                JwtClaimNames.Name ,
                displayName)
        };

        if ( !string.IsNullOrWhiteSpace(user.Email) )
        {
            claims.Add(
                new Claim(
                    JwtClaimNames.Email ,
                    user.Email));
        }

        if ( !string.IsNullOrWhiteSpace(user.PhoneNumber) )
        {
            claims.Add(
                new Claim(
                    JwtClaimNames.PhoneNumber ,
                    user.PhoneNumber));
        }

        var roles = await _userManager.GetRolesAsync(user);

        foreach ( var role in roles )
        {
            claims.Add(
                new Claim(
                    JwtClaimNames.Role ,
                    role));
        }

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _jwtSettings.Issuer ,
            Audience = _jwtSettings.Audience ,

            Subject = new ClaimsIdentity(claims) ,

            IssuedAt = now ,
            NotBefore = now ,
            Expires = expiresAtUtc ,

            SigningCredentials = _signingCredentials
        };

        var token = _tokenHandler.CreateToken(descriptor);

        return new AccessTokenResult(
            token ,
            expiresAtUtc);
    }

    public RefreshTokenResult CreateRefreshToken()
    {
        var now = _timeProvider.GetUtcNow().UtcDateTime;

        var randomBytes =
            RandomNumberGenerator.GetBytes(64);

        var rawToken =
            Base64UrlEncoder.Encode(randomBytes);

        var tokenHash =
            HashRefreshToken(rawToken);

        var expiresAtUtc = now.AddDays(
            _jwtSettings.RefreshTokenDays);

        return new RefreshTokenResult(
            rawToken ,
            tokenHash ,
            expiresAtUtc);
    }

    public string HashRefreshToken(
        string refreshToken )
    {
        if ( string.IsNullOrWhiteSpace(refreshToken) )
        {
            throw new ArgumentException(
                "Refresh token cannot be empty." ,
                nameof(refreshToken));
        }

        var tokenBytes =
            Encoding.UTF8.GetBytes(refreshToken);

        var hashBytes =
            SHA256.HashData(tokenBytes);

        return Convert.ToHexString(hashBytes);
    }

    private static string GetDisplayName(
        ApplicationUser user ,
        string userId )
    {
        if ( !string.IsNullOrWhiteSpace(user.FullNameEn) )
        {
            return user.FullNameEn;
        }

        if ( !string.IsNullOrWhiteSpace(user.FullNameAr) )
        {
            return user.FullNameAr;
        }

        if ( !string.IsNullOrWhiteSpace(user.UserName) )
        {
            return user.UserName;
        }

        if ( !string.IsNullOrWhiteSpace(user.PhoneNumber) )
        {
            return user.PhoneNumber;
        }

        if ( !string.IsNullOrWhiteSpace(user.Email) )
        {
            return user.Email;
        }

        return userId;
    }
}
