import { useQuery } from "@tanstack/react-query";
import { getCategories } from "../api/get-categories";
import type { CategoryQueryParams } from "../types/category-query-params";

export function useCategories(
  params: CategoryQueryParams
) {
  const {
    data,
    isPending: isLoading,
    error,
  } = useQuery({
    queryKey: ["categories", params],

    queryFn: () => getCategories(params),
  });

  return {
    data,
    isLoading,
    error,
  };
}
