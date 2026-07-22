import { api } from "@/lib/axios";
import type { Category } from "../types/category";

export async function restoreCategory(
  id: number
) {
  const response =
    await api.post<Category>(
      `/categories/${id}/restore`
    );

  return response.data;
}
