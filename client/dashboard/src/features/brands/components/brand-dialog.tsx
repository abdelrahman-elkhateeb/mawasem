import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

import { BrandForm } from "./brand-form";
import type { BrandDialogProps } from "./types";

export function BrandDialog({
  open,
  onOpenChange,
  mode,
  brand,
}: BrandDialogProps) {
  const isEditMode = mode === "edit";

  const title = isEditMode
    ? "Edit Brand"
    : "Add Brand";

  const description = isEditMode
    ? "Update brand details and save your changes."
    : "Create a new brand by filling the details below.";

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{title}</DialogTitle>
          <DialogDescription>{description}</DialogDescription>
        </DialogHeader>

        <BrandForm
          mode={mode}
          brand={brand}
          onSuccess={() => onOpenChange(false)}
          onCancel={() => onOpenChange(false)}
        />
      </DialogContent>
    </Dialog>
  );
}