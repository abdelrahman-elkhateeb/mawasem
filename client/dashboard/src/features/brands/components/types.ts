import type { Brand } from "../types/brand";
import type { BrandFormValues } from "../schema/brand-form-schema";

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
  formId: string;
  errorMessage?: string | null;
  onSubmit: (
    values: BrandFormValues
  ) => Promise<void>;
}

export interface BrandActionsProps {
  brand: Brand;
}

export interface DeleteBrandDialogProps {
  brandId: number;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}