import { Button } from "@/components/ui/button";

import type { EntityDialogFooterProps } from "./types";

export function EntityDialogFooter({
  mode,
  formId,
  isLoading = false,
  onCancel,
  cancelLabel = "Cancel",
  createLabel = "Create",
  createLoadingLabel = "Creating...",
  editLabel = "Save changes",
  editLoadingLabel = "Saving...",
}: EntityDialogFooterProps) {
  const submitLabel =
    mode === "create"
      ? createLabel
      : editLabel;

  const loadingLabel =
    mode === "create"
      ? createLoadingLabel
      : editLoadingLabel;

  return (
    <div className="flex justify-end gap-2">
      <Button
        type="button"
        variant="outline"
        onClick={onCancel}
        disabled={isLoading}
      >
        {cancelLabel}
      </Button>

      <Button
        type="submit"
        form={formId}
        disabled={isLoading}
      >
        {isLoading
          ? loadingLabel
          : submitLabel}
      </Button>
    </div>
  );
}
