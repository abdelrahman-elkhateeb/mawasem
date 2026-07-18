import { createBrowserRouter } from "react-router-dom";

import { adminRoutes } from "./admin.routes";
import { authRoutes } from "./auth.routes";
import { storeRoutes } from "./store.routes";

export const router = createBrowserRouter([
  adminRoutes,
  authRoutes,
  storeRoutes
]);