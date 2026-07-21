import { useQuery } from "@tanstack/react-query";
import { getBrands } from "../api/get-brands";

interface UseBrandsParams {
  search?: string;
  isActive?: boolean;
  includeDeleted?: boolean;
  pageNumber: number;
  pageSize: number;
}

export function useBrands(
  params: UseBrandsParams
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