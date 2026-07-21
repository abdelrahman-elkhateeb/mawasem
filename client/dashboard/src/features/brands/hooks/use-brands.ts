import { useQuery } from "@tanstack/react-query";
import { getBrands } from "../api/get-brands";

export function useBrands(page: number, pageSize: number) {
  const { data: brands, isPending: isLoading, error } = useQuery({
    queryKey: ["brands", page, pageSize],
    queryFn: () => getBrands({
      page,
      pageSize
    })
  })

  return { brands, isLoading, error }
}