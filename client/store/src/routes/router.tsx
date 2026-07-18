import { createBrowserRouter } from "react-router-dom"

import { authRoutes } from "./auth.routes"
import { storeRoutes } from "./store.routes"

export const router = createBrowserRouter([storeRoutes, authRoutes])
