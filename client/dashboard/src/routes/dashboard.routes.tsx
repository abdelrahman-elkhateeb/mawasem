import AdminLayout from "@/layouts/AdminLayout";
import DashboardPage from "@/pages/Home/DashboardPage";
import type { RouteObject } from "react-router-dom";
import { ProtectedRoute } from "./protected-route";
import BrandsPage from "@/features/brands/pages/BrandsPage";
import CategoriesPage from "@/features/categories/pages/CategoriesPage";
import CollectionsPage from "@/features/collections/pages/CollectionsPage";

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
      path: "brands",
      element: <BrandsPage />
    },
    {
      path: 'categories',
      element: <CategoriesPage />
    },
    {
      path: "collections",
      element: <CollectionsPage />
    }
  ],
};
