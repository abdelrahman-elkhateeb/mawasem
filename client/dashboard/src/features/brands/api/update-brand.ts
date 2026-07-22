import { api } from "@/lib/axios";
import type { Brand, UpdateBrandParams } from "../types/brand";

export async function updateBrand({
  id,
  data,
}: UpdateBrandParams) {
  const response = await api.put<Brand>(
    `/brands/${id}`,
    data
  );

  return response.data;
}