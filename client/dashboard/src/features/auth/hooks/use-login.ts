
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { login } from "../api/login";

export function useLogin() {
  const queryClient = useQueryClient();

  const { isPending: isUserLoading, mutate: loginUser, error } = useMutation({
    mutationFn: login,

    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: ["me"],
      });
    }
  });

  return { isUserLoading, loginUser, error }
}