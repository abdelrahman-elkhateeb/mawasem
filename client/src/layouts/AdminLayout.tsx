import { Outlet } from "react-router-dom";
import AdminSidebar from "@/components/admin/AdminSideBar";

export default function AdminLayout() {
  return (
    <div className="flex min-h-screen bg-muted/40">
      <AdminSidebar />

      <main className="min-w-0 flex-1 p-6">
        <Outlet />
      </main>
    </div>
  );
}