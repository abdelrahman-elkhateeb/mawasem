import { api } from "@/lib/axios";
import type { Category } from "../types/category";

export async function getCategory(id: number) {
  const response =
    await api.get<Category>(
      `/categories/${id}`
    );

  return response.data;
}
