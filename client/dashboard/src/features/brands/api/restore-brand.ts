import { api } from "@/lib/axios";
import type { Brand } from "../types/brand";

export async function restoreBrand(
  id: number
) {
  const response =
    await api.post<Brand>(
      `/brands/${id}/restore`
    );

  return response.data;
}
