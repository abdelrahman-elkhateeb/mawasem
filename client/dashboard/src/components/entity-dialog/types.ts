import type { ReactNode } from "react";

export type EntityDialogMode =
  | "create"
  | "edit";

export interface EntityDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  title: string;
  description: string;
  children: ReactNode;
}

export interface EntityDialogFooterProps {
  mode: EntityDialogMode;
  formId: string;
  isLoading?: boolean;
  onCancel: () => void;
  cancelLabel?: string;
  createLabel?: string;
  createLoadingLabel?: string;
  editLabel?: string;
  editLoadingLabel?: string;
}
