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

  if (!data?.user) {
    return <Navigate to="/auth/login" replace />;
  }

  return children;
}