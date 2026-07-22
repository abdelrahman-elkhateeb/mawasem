import type { PaginatedResponse } from "@/types/pagination";

export interface Category {
  id: number;
  nameAr: string;
  nameEn: string;
  productCount: number;
  isDeleted: boolean;
  createdOn: string;
  createdBy: string;
  lastModifiedOn: string | null;
  lastModifiedBy: string | null;
  deletedOn: string | null;
  deletedBy: string | null;
}

export interface CategoryPayload {
  nameAr: string;
  nameEn: string;
}

export interface UpdateCategoryParams {
  id: number;
  data: CategoryPayload;
}

export type CategoriesResponse =
  PaginatedResponse<Category>;
