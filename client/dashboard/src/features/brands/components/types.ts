import type { Brand } from "../types/brand";

export type BrandDialogMode =
  | "create"
  | "edit";

export interface BrandDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  mode: BrandDialogMode;
  brand?: Brand;
}

export interface BrandFormProps {
  mode: BrandDialogMode;
  brand?: Brand;
  onSuccess: () => void;
  onCancel: () => void;
}

export interface BrandActionsProps {
  brand: Brand;
}

export interface DeleteBrandDialogProps {
  brandId: number;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}