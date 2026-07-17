import StoreLayout from "@/layouts/StoreLayout";
import HomePage from "@/pages/store/HomePage";
import type { RouteObject } from "react-router";

export const storeRoutes: RouteObject = {
  path: "/",
  element: <StoreLayout />,
  children: [
    {
      index: true,
      element: <HomePage />
    }

  ]
}