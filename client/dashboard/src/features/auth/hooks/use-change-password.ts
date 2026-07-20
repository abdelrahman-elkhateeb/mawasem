import { useMutation } from "@tanstack/react-query";
import { useNavigate } from "react-router-dom";
import { changePassword as changePasswordApi } from "../api/change-password";

export function useChangePassword() {
  const navigate = useNavigate();

  const { mutate: changePassword, isPending: isLoading } = useMutation({
    mutationFn: changePasswordApi,
    onSuccess: async () => {
      navigate("/auth/login", {
        replace: true
      })
    }
  })

  return { changePassword, isLoading }
}