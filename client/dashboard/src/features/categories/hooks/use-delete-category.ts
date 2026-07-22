import { useMutation, useQueryClient } from "@tanstack/react-query";
import { deleteCategory } from "../api/delete-category";

export function useDeleteCategory() {
  const queryClient = useQueryClient();

  const {
    mutate: deleteCategoryMutation,
    mutateAsync: deleteCategoryMutationAsync,
    isPending: isLoading,
    error,
  } = useMutation({
    mutationFn: deleteCategory,

    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["categories"],
      });
    },
  });

  return {
    deleteCategoryMutation,
    deleteCategoryMutationAsync,
    isLoading,
    error,
  };
}
