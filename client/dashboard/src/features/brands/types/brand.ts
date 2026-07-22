import type { PaginatedResponse } from "@/types/pagination";

export interface Brand {
  id: number;
  nameAr: string;
  nameEn: string;
  descriptionAr: string;
  descriptionEn: string;
  logoUrl: string;
  isActive: boolean;
}

export interface BrandPayload {
  nameAr: string;
  nameEn: string;
  descriptionAr: string;
  descriptionEn: string;
  logoUrl: string;
  isActive: boolean;
}

export interface UpdateBrandParams {
  id: number;
  data: BrandPayload;
}

export type BrandsResponse =
  PaginatedResponse<Brand>;
