import type { RouteObject } from "react-router-dom";
import DashboardPage from "@/pages/admin/DashboardPage"
import AdminLayout from "@/layouts/AdminLayout";

export const adminRoutes: RouteObject = {
  path: "/",
  element: <AdminLayout />,
  children: [
    {
      index: true,
      element: <DashboardPage />,
    },
  ],
};
