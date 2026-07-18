import { AppSidebar } from "@/components/app-sidebar";
import { SidebarProvider, SidebarTrigger } from "@/components/ui/sidebar";
import { Outlet } from "react-router-dom";


export default function AdminLayout() {
  return (
    <div className="flex min-h-screen bg-muted/40">
      <SidebarProvider>
        <AppSidebar />
        <SidebarTrigger className="-ml-1" />
        
        <main className="min-w-0 flex-1 p-6">
          <Outlet />
        </main>
      </SidebarProvider>
    </div>
  );
}