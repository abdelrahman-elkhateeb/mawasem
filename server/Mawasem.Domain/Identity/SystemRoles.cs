namespace Mawasem.Domain.Identity;

public static class SystemRoles
{
    public const string Customer = "Customer";

    public const string SuperAdmin = "SuperAdmin";

    public const string Admin = "Admin";

    public const string SalesEmployee = "SalesEmployee";

    public const string DeliveryEmployee = "DeliveryEmployee";

    public const string SupportEmployee = "SupportEmployee";

    public const string StoreEmployee = "StoreEmployee";

    private static readonly HashSet<string> DashboardRoleSet =
        new(StringComparer.OrdinalIgnoreCase)
        {
            SuperAdmin,
            Admin,
            SalesEmployee,
            DeliveryEmployee,
            SupportEmployee,
            StoreEmployee
        };

    public static IReadOnlyCollection<string> DashboardRoles =>
        DashboardRoleSet;

    public static IReadOnlyCollection<string> All { get; } =
        new[]
        {
            Customer,
            SuperAdmin,
            Admin,
            SalesEmployee,
            DeliveryEmployee,
            SupportEmployee,
            StoreEmployee
        };

    public static bool IsDashboardRole( string? roleName )
    {
        return !string.IsNullOrWhiteSpace(roleName) &&
               DashboardRoleSet.Contains(roleName);
    }
}