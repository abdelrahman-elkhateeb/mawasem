import { api } from "@/lib/axios";
import type { PaginatedResponse } from "../types/pagination";
import type { Brand } from "../types/brands";

interface GetBrandsParams {
  page: number;
  pageSize: number;
}

export async function getBrands({
  page, pageSize
}: GetBrandsParams) {
  const response = await api.get<PaginatedResponse<Brand>>("/brands",
    {
      params: {
        page,
        pageSize,
      },
    })

  return response.data;
}