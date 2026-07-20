import ChangePasswordPage from "@/features/auth/pages/ChangePasswordPage";
import LoginPage from "@/features/auth/pages/LoginPage";
import AuthLayout from "@/layouts/AuthLayout";
import type { RouteObject } from "react-router-dom";

export const authRoutes: RouteObject = {
  path: "/auth",
  element: <AuthLayout />,
  children: [
    {
      path: "login",
      element: <LoginPage />,
    },
    {
      path: "change-password",
      element: <ChangePasswordPage />,
    }
  ],
};
