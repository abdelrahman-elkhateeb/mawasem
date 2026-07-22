import { useQuery } from "@tanstack/react-query";
import { getBrands } from "../api/get-brands";
import type { BrandQueryParams } from "../types/brand-query-params";

export function useBrands(
  params: BrandQueryParams
) {
  const {
    data,
    isPending: isLoading,
    error,
  } = useQuery({
    queryKey: ["brands", params],

    queryFn: () => getBrands(params),
  });

  return {
    data,
    isLoading,
    error,
  };
}