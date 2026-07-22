import { useQuery } from "@tanstack/react-query";
import { getCategory } from "../api/get-category";

export function useCategory(id: number) {
  const {
    data: category,
    isLoading,
    error,
  } = useQuery({
    queryKey: ["category", id],

    queryFn: () => getCategory(id),

    enabled: !!id,
  });

  return {
    category,
    isLoading,
    error,
  };
}
