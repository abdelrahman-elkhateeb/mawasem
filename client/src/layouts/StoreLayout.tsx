import StoreNavbar from "@/components/Navbar";
import { Outlet } from "react-router-dom";

export default function StoreLayout() {
  return (
    <main >
      <StoreNavbar />
      <div className="container mx-auto px-4">
        <Outlet />
      </div>
    </main>
  );
}