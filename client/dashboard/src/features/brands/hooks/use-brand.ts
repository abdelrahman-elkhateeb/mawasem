import { useQuery } from "@tanstack/react-query";
import { getBrand } from "../api/get-brand";

export function useBrand(id: number) {
  const {
    data: brand,
    isLoading,
    error,
  } = useQuery({
    queryKey: ["brand", id],

    queryFn: () => getBrand(id),

    enabled: !!id,
  });

  return {
    brand,
    isLoading,
    error,
  };
}