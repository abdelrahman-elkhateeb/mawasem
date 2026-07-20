import { useMutation, useQueryClient } from "@tanstack/react-query";
import { logout as LogoutApi } from "../api/logout";
import { useNavigate } from "react-router-dom";

export function useLogout() {
  const queryClient = useQueryClient();
  const navigation = useNavigate();

  const { mutate: logout, isPending: isLoading } = useMutation({
    mutationFn: LogoutApi,

    onSuccess: () => {
      queryClient.clear();
      navigation("/auth/login", { replace: true })
    },
  })

  return { logout, isLoading };
}