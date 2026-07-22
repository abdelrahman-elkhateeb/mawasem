import { api } from "@/lib/axios";

export async function deleteCategory(
  id: number
) {
  await api.delete(
    `/categories/${id}`
  );
}
