import { useMutation, useQueryClient } from "@tanstack/react-query";
import { restoreCategory } from "../api/restore-category";

export function useRestoreCategory() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: restoreCategory,

    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["categories"],
      });
    },
  });
}
