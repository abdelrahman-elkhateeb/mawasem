import type { PaginatedResponse } from "./pagination";

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

export type BrandsResponse =
  PaginatedResponse<Brand>;