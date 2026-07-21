import { api } from "@/lib/axios";
import type { Brand, BrandPayload } from "../types/brands";

export async function createBrand(
  data: BrandPayload
) {
  const response = await api.post<Brand>(
    "/brands",
    data
  );

  return response.data;
}