import { createBrowserRouter } from "react-router-dom";

import { adminRoutes } from "./admin.routes";
import { authRoutes } from "./auth.routes";

export const router = createBrowserRouter([
  adminRoutes,
  authRoutes,
]);