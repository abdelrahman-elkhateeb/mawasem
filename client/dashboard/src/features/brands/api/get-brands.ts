import { api } from "@/lib/axios";
import type { PaginatedResponse } from "@/types/pagination";
import type { BrandQueryParams } from "../types/brand-query-params";
import type { Brand } from "../types/brand";

export async function getBrands({
  search,
  isActive,
  includeDeleted,
  pageNumber,
  pageSize,
}: BrandQueryParams) {
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