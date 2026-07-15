namespace Mawasem.Application.Features.Roles.Models;

public static class RoleManagementErrorCodes
{
    public const string InvalidRequest =
        "roles.invalid_request";

    public const string NotFound =
        "roles.not_found";

    public const string Forbidden =
        "roles.forbidden";

    public const string ProtectedRole =
        "roles.protected_role";

    public const string InvalidPermission =
        "roles.invalid_permission";

    public const string UpdateFailed =
        "roles.update_failed";
}