import { api } from "@/lib/axios";
import type {
  Category,
  UpdateCategoryParams,
} from "../types/category";

export async function updateCategory({
  id,
  data,
}: UpdateCategoryParams) {
  const response =
    await api.put<Category>(
      `/categories/${id}`,
      data
    );

  return response.data;
}
