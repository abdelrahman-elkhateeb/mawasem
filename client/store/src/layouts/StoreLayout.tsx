import { Outlet } from "react-router-dom";

export default function StoreLayout() {
  return (
    <main className="min-h-screen">
      <Outlet />
    </main>
  );
}
