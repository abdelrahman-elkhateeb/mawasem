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

    public const string CurrentPasswordIncorrect =
        "authentication.current_password_incorrect";

    public const string PasswordConfirmationMismatch =
        "authentication.password_confirmation_mismatch";

    public const string PasswordChangeFailed =
        "authentication.password_change_failed";

    public const string PasswordResetCodeInvalid =
        "authentication.password_reset_code_invalid";

    public const string PasswordResetTokenInvalid =
        "authentication.password_reset_token_invalid";

    public const string PasswordResetFailed =
        "authentication.password_reset_failed";

    public const string PasswordResetDeliveryFailed =
        "authentication.password_reset_delivery_failed";
}