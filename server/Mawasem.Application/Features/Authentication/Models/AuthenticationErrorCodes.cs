namespace Mawasem.Application.Features.Authentication.Models;

public static class AuthenticationErrorCodes
{
    public const string InvalidRequest =
        "authentication.invalid_request";

    public const string InvalidPhoneNumber =
        "authentication.invalid_phone_number";

    public const string PhoneAlreadyRegistered =
        "authentication.phone_already_registered";

    public const string InvalidCredentials =
        "authentication.invalid_credentials";

    public const string InvalidRefreshToken =
        "authentication.invalid_refresh_token";

    public const string AccountBlocked =
        "authentication.account_blocked";

    public const string AccountLocked =
        "authentication.account_locked";

    public const string RegistrationFailed =
        "authentication.registration_failed";
}