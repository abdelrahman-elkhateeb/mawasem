import { Outlet } from "react-router-dom";

export default function StoreLayout() {
  return (
    <main className="container mx-auto px-4">
      <Outlet />
    </main>
  );
}