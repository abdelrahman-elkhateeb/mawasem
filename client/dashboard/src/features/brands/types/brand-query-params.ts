export interface BrandQueryParams {
  search?: string;
  isActive?: boolean;
  includeDeleted?: boolean;
  pageNumber: number;
  pageSize: number;
}