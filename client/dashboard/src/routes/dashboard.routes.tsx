import AdminLayout from "@/layouts/AdminLayout";
import DashboardPage from "@/pages/Home/DashboardPage";
import type { RouteObject } from "react-router-dom";
import { ProtectedRoute } from "./protected-route";
import ChangePasswordPage from "@/features/auth/pages/changePasswordPage";

export const dashboardRoutes: RouteObject = {
  path: "/",
  element: (
    <ProtectedRoute>
      <AdminLayout />
    </ProtectedRoute>
  ),
  children: [
    {
      index: true,
      element: <DashboardPage />,
    },
    {
      path: "change-password",
      element: <ChangePasswordPage />,
    },
  ],
};
