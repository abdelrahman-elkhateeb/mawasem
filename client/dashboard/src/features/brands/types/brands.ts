import type { PaginatedResponse } from "./pagination";

export interface Brand {
  id: number;
  nameAr: string;
  nameEn: string;
}

export type BrandsResponse =
  PaginatedResponse<Brand>;