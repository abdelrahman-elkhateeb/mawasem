import { createBrowserRouter } from "react-router-dom";

import { dashboardRoutes } from "./dashboard.routes";
import { authRoutes } from "./auth.routes";

export const router = createBrowserRouter([dashboardRoutes, authRoutes]);
