import { useMutation, useQueryClient } from "@tanstack/react-query";
import { createCategory } from "../api/create-category";

export function useCreateCategory() {
  const queryClient = useQueryClient();

  const {
    mutate: createCategoryMutation,
    mutateAsync: createCategoryMutationAsync,
    isPending: isLoading,
    error,
  } = useMutation({
    mutationFn: createCategory,

    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["categories"],
      });
    },
  });

  return {
    createCategoryMutation,
    createCategoryMutationAsync,
    isLoading,
    error,
  };
}
