import { useMutation, useQueryClient } from "@tanstack/react-query";
import { restoreBrand } from "../api/restore-brand";

export function useRestoreBrand() {
  const queryClient = useQueryClient();

  const {
    mutate: restoreBrandMutation,
    mutateAsync: restoreBrandMutationAsync,
    isPending: isLoading,
    error,
  } = useMutation({
    mutationFn: restoreBrand,

    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["brands"],
      });
    },
  });

  return {
    restoreBrandMutation,
    restoreBrandMutationAsync,
    isLoading,
    error,
  };
}
