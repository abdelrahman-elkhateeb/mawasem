import { useMutation, useQueryClient } from "@tanstack/react-query";
import { updateCategory } from "../api/update-category";

export function useUpdateCategory() {
  const queryClient = useQueryClient();

  const {
    mutate: updateCategoryMutation,
    mutateAsync: updateCategoryMutationAsync,
    isPending: isLoading,
    error,
  } = useMutation({
    mutationFn: updateCategory,

    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["categories"],
      });
    },
  });

  return {
    updateCategoryMutation,
    updateCategoryMutationAsync,
    isLoading,
    error,
  };
}
