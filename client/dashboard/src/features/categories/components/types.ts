import type { EntityDialogMode } from "@/components/entity-dialog/types";
import type { CategoryFormValues } from "../schema/category-form-schema";
import type { Category } from "../types/category";

export type CategoryDialogMode =
  EntityDialogMode;

export interface CategoryDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  mode: CategoryDialogMode;
  category?: Category;
}

export interface CategoryFormProps {
  mode: CategoryDialogMode;
  category?: Category;
  formId: string;
  errorMessage?: string | null;
  onSubmit: (
    values: CategoryFormValues
  ) => Promise<void>;
}

export interface CategoryColumnActions {
  onEdit: (category: Category) => void;
  onDelete: (categoryId: number) => void;
  onRestore: (categoryId: number) => void;
}
