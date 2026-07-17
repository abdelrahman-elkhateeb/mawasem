import { NavLink } from "react-router-dom";
import {
  LayoutDashboard,
  Package,
  Shapes,
  ShoppingCart,
} from "lucide-react";

const sidebarLinks = [
  {
    label: "Dashboard",
    path: "/admin",
    icon: LayoutDashboard,
  },
  {
    label: "Products",
    path: "/admin/products",
    icon: Package,
  },
  {
    label: "Categories",
    path: "/admin/categories",
    icon: Shapes,
  },
  {
    label: "Orders",
    path: "/admin/orders",
    icon: ShoppingCart,
  },
];

export default function AdminSidebar() {
  return (
    <aside className="min-h-screen w-64 shrink-0 border-r bg-background p-4">
      <div className="mb-8 px-3">
        <h2 className="text-xl font-bold">Mawasem</h2>
        <p className="text-sm text-muted-foreground">
          Admin Dashboard
        </p>
      </div>

      <nav className="space-y-2">
        {sidebarLinks.map((link) => {
          const Icon = link.icon;

          return (
            <NavLink
              key={link.path}
              to={link.path}
              end={link.path === "/admin"}
              className={({ isActive }) =>
                [
                  "flex items-center gap-3 rounded-md px-3 py-2 text-sm font-medium transition-colors",
                  isActive
                    ? "bg-primary text-primary-foreground"
                    : "text-muted-foreground hover:bg-muted hover:text-foreground",
                ].join(" ")
              }
            >
              <Icon className="size-4" />
              <span>{link.label}</span>
            </NavLink>
          );
        })}
      </nav>
    </aside>
  );
}