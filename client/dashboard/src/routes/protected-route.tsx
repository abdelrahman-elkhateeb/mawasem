import { useMe } from "@/features/auth/hooks/use-me";
import { Navigate } from "react-router-dom";


export function ProtectedRoute({
  children,
}: {
  children: React.ReactNode;
}) {
  const { data, isLoading } = useMe();

  if (isLoading) {
    return <p>Loading...</p>;
  }

  if (!data) {
    return <Navigate to="/auth/login" replace />;
  }

  if (
    data.user.mustChangePassword &&
    location.pathname !== "/change-password"
  ) {
    return (
      <Navigate
        to="/change-password"
        replace
      />
    );
  }
  return children;
}