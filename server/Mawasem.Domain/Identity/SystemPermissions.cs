namespace Mawasem.Domain.Identity;

public static class SystemPermissions
{
    public static class Dashboard
    {
        public const string Access = "Dashboard.Access";
    }

    public static class Products
    {
        public const string View = "Products.View";
        public const string Create = "Products.Create";
        public const string Edit = "Products.Edit";
        public const string Delete = "Products.Delete";
        public const string Restore = "Products.Restore";
        public const string ManageStock = "Products.ManageStock";
        public const string ManageImages = "Products.ManageImages";
    }

    public static class Categories
    {
        public const string View = "Categories.View";
        public const string Create = "Categories.Create";
        public const string Edit = "Categories.Edit";
        public const string Delete = "Categories.Delete";
        public const string Restore = "Categories.Restore";
    }

    public static class Brands
    {
        public const string View = "Brands.View";
        public const string Create = "Brands.Create";
        public const string Edit = "Brands.Edit";
        public const string Delete = "Brands.Delete";
        public const string Restore = "Brands.Restore";
    }

    public static class Seasons
    {
        public const string View = "Seasons.View";
        public const string Create = "Seasons.Create";
        public const string Edit = "Seasons.Edit";
        public const string Delete = "Seasons.Delete";
        public const string Restore = "Seasons.Restore";
    }

    public static class Collections
    {
        public const string View = "Collections.View";
        public const string Create = "Collections.Create";
        public const string Edit = "Collections.Edit";
        public const string Delete = "Collections.Delete";
        public const string Restore = "Collections.Restore";
    }

    public static class Orders
    {
        public const string View = "Orders.View";
        public const string CreateStoreOrder = "Orders.CreateStoreOrder";
        public const string UpdateStatus = "Orders.UpdateStatus";
        public const string Cancel = "Orders.Cancel";
    }

    public static class Refunds
    {
        public const string View = "Refunds.View";
        public const string Review = "Refunds.Review";
        public const string Approve = "Refunds.Approve";
        public const string Reject = "Refunds.Reject";
    }

    public static class Reviews
    {
        public const string View = "Reviews.View";
        public const string Delete = "Reviews.Delete";
    }

    public static class Customers
    {
        public const string View = "Customers.View";
        public const string Block = "Customers.Block";
        public const string Unblock = "Customers.Unblock";
    }

    public static class Employees
    {
        public const string View = "Employees.View";
        public const string Create = "Employees.Create";
        public const string Edit = "Employees.Edit";
        public const string Block = "Employees.Block";
        public const string Unblock = "Employees.Unblock";
        public const string ResetPassword = "Employees.ResetPassword";
        public const string AssignRoles = "Employees.AssignRoles";
        public const string AssignPermissions =
            "Employees.AssignPermissions";
    }

    public static class Roles
    {
        public const string View = "Roles.View";
        public const string ManagePermissions =
            "Roles.ManagePermissions";
    }

    public static class DeliveryAreas
    {
        public const string View = "DeliveryAreas.View";
        public const string Create = "DeliveryAreas.Create";
        public const string Edit = "DeliveryAreas.Edit";
        public const string Delete = "DeliveryAreas.Delete";
    }

    public static class Payments
    {
        public const string View = "Payments.View";
        public const string Refund = "Payments.Refund";
    }

    public static class Reports
    {
        public const string View = "Reports.View";
        public const string Export = "Reports.Export";
    }

    public static class Complaints
    {
        public const string View = "Complaints.View";
        public const string Create = "Complaints.Create";
        public const string Edit = "Complaints.Edit";
        public const string Assign = "Complaints.Assign";
        public const string Close = "Complaints.Close";
    }

    public static IReadOnlyCollection<string> All { get; } =
        new[]
        {
            Dashboard.Access,

            Products.View,
            Products.Create,
            Products.Edit,
            Products.Delete,
            Products.Restore,
            Products.ManageStock,
            Products.ManageImages,

            Categories.View,
            Categories.Create,
            Categories.Edit,
            Categories.Delete,
            Categories.Restore,

            Brands.View,
            Brands.Create,
            Brands.Edit,
            Brands.Delete,
            Brands.Restore,

            Seasons.View,
            Seasons.Create,
            Seasons.Edit,
            Seasons.Delete,
            Seasons.Restore,

            Collections.View,
            Collections.Create,
            Collections.Edit,
            Collections.Delete,
            Collections.Restore,

            Orders.View,
            Orders.CreateStoreOrder,
            Orders.UpdateStatus,
            Orders.Cancel,

            Refunds.View,
            Refunds.Review,
            Refunds.Approve,
            Refunds.Reject,

            Reviews.View,
            Reviews.Delete,

            Customers.View,
            Customers.Block,
            Customers.Unblock,

            Employees.View,
            Employees.Create,
            Employees.Edit,
            Employees.Block,
            Employees.Unblock,
            Employees.ResetPassword,
            Employees.AssignRoles,
            Employees.AssignPermissions,

            Roles.View,
            Roles.ManagePermissions,

            DeliveryAreas.View,
            DeliveryAreas.Create,
            DeliveryAreas.Edit,
            DeliveryAreas.Delete,

            Payments.View,
            Payments.Refund,

            Reports.View,
            Reports.Export,

            Complaints.View,
            Complaints.Create,
            Complaints.Edit,
            Complaints.Assign,
            Complaints.Close
        };
}