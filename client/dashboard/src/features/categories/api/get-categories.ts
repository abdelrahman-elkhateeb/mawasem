import { api } from "@/lib/axios";
import type { PaginatedResponse } from "@/types/pagination";
import type { Category } from "../types/category";
import type { CategoryQueryParams } from "../types/category-query-params";

export async function getCategories({
  search,
  includeDeleted,
  pageNumber,
  pageSize,
}: CategoryQueryParams) {
  const response =
    await api.get<
      PaginatedResponse<Category>
    >("/categories", {
      params: {
        search,
        includeDeleted,
        pageNumber,
        pageSize,
      },
    });

  return response.data;
}
