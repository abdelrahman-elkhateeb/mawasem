import { api } from "@/lib/axios";
import type {
  Category,
  CategoryPayload,
} from "../types/category";

export async function createCategory(
  data: CategoryPayload
) {
  const response =
    await api.post<Category>(
      "/categories",
      data
    );

  return response.data;
}
