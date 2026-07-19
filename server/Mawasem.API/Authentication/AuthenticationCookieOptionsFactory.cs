namespace Mawasem.API.Authentication;

public static class AuthenticationCookieOptionsFactory
{
    public static CookieOptions CreateAccessToken(
        DateTime expiresAtUtc )
    {
        var utcExpiration =
            DateTime.SpecifyKind(
                expiresAtUtc ,
                DateTimeKind.Utc);

        return Create(
            new DateTimeOffset(utcExpiration) ,
            "/");
    }

    public static CookieOptions CreateRefreshToken(
        DateTimeOffset expiresAtUtc ,
        string path )
    {
        return Create(
            expiresAtUtc ,
            path);
    }

    public static CookieOptions CreateDeletion(
        string path )
    {
        return Create(
            expiresAtUtc: null ,
            path);
    }

    private static CookieOptions Create(
        DateTimeOffset? expiresAtUtc ,
        string path )
    {
        return new CookieOptions
        {
            HttpOnly = true ,
            Secure = true ,
            SameSite = SameSiteMode.Strict ,
            IsEssential = true ,
            Expires = expiresAtUtc ,
            Path = path
        };
    }
}