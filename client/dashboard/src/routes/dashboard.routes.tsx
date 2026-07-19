import AdminLayout from "@/layouts/AdminLayout";
import DashboardPage from "@/pages/Home/DashboardPage";
import type { RouteObject } from "react-router-dom";

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
