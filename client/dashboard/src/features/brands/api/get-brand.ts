import { api } from "@/lib/axios";
import type { Brand } from "../types/brand";

export async function getBrand(id: number) {
  const response = await api.get<Brand>(`/brand/${id}`)

  return response.data;
}