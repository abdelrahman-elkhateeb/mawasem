import { api } from "@/lib/axios";
import type { PaginatedResponse } from "../types/pagination";
import type { Brand } from "../types/brands";

interface GetBrandsParams {
  search?: string;
  isActive?: boolean;
  includeDeleted?: boolean;
  pageNumber: number;
  pageSize: number;
}

export async function getBrands({
  search,
  isActive,
  includeDeleted,
  pageNumber,
  pageSize,
}: GetBrandsParams) {
  const response =
    await api.get<
      PaginatedResponse<Brand>
    >("/brands", {
      params: {
        search,
        isActive,
        includeDeleted,
        pageNumber,
        pageSize,
      },
    });

  return response.data;
}