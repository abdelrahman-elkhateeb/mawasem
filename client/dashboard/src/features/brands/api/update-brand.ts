import { api } from "@/lib/axios";
import type { Brand, BrandPayload } from "../types/brands";

interface UpdateBrandParams {
  id: number;
  data: BrandPayload;
}

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