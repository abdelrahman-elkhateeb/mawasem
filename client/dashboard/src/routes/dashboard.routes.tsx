import type { RouteObject } from "react-router-dom";
import DashboardPage from "@/pages/Home/DashboardPage"
import AdminLayout from "@/layouts/AdminLayout";

export const dashboardRoutes: RouteObject = {
  path: "/",
  element: <AdminLayout />,
  children: [
    {
      index: true,
      element: <DashboardPage />,
    },
  ],
};
