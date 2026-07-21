import { api } from "@/lib/axios";

export async function deleteBrand(
  id: number
) {
  await api.delete(`/brands/${id}`);
}