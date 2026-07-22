import { useMutation, useQueryClient } from "@tanstack/react-query";
import { restoreCategory } from "../api/restore-category";

export function useRestoreCategory() {
  const queryClient = useQueryClient();

  const {
    mutate: restoreCategoryMutation,
    mutateAsync: restoreCategoryMutationAsync,
    isPending: isLoading,
    error,
  } = useMutation({
    mutationFn: restoreCategory,

    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["categories"],
      });
    },
  });

  return {
    restoreCategoryMutation,
    restoreCategoryMutationAsync,
    isLoading,
    error,
  };
}
