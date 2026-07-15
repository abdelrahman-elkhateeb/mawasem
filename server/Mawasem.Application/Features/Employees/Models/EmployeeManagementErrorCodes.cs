namespace Mawasem.Application.Features.Employees.Models;

public static class EmployeeManagementErrorCodes
{
    public const string InvalidRequest =
        "employees.invalid_request";

    public const string NotFound =
        "employees.not_found";

    public const string EmailAlreadyRegistered =
        "employees.email_already_registered";

    public const string InvalidRole =
        "employees.invalid_role";

    public const string InvalidPermission =
        "employees.invalid_permission";

    public const string Forbidden =
        "employees.forbidden";

    public const string CannotManageSuperAdmin =
        "employees.cannot_manage_super_admin";

    public const string CannotManageSelf =
        "employees.cannot_manage_self";

    public const string PasswordConfirmationMismatch =
        "employees.password_confirmation_mismatch";

    public const string CreationFailed =
        "employees.creation_failed";

    public const string UpdateFailed =
        "employees.update_failed";

    public const string RoleAssignmentFailed =
        "employees.role_assignment_failed";

    public const string PermissionAssignmentFailed =
        "employees.permission_assignment_failed";

    public const string PasswordResetFailed =
        "employees.password_reset_failed";
}