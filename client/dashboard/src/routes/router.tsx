import { createBrowserRouter } from "react-router-dom";

import { dashboardRoutes } from "./dashboard.routes";
import { authRoutes } from "./auth.routes";
import NotFoundPage from "@/pages/NotFoundPage";

export const router = createBrowserRouter([
  dashboardRoutes,
  authRoutes,
  {
    path: "*",
    element: <NotFoundPage />,
  },
]);
