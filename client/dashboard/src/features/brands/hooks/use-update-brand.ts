import { useMutation, useQueryClient } from "@tanstack/react-query";
import { updateBrand } from "../api/update-brand";

export function useUpdateBrand() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: updateBrand,

    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["brands"],
      });
    },
  });
}