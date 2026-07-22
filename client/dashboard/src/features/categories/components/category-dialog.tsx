import { EntityDialog } from "@/components/entity-dialog/entity-dialog";
import { EntityDialogFooter } from "@/components/entity-dialog/entity-dialog-footer";

import { useCreateCategory } from "../hooks/use-create-category";
import { useUpdateCategory } from "../hooks/use-update-category";
import type { CategoryFormValues } from "../schema/category-form-schema";
import { CategoryForm } from "./category-form";
import type { CategoryDialogProps } from "./types";

export function CategoryDialog({
  open,
  onOpenChange,
  mode,
  category,
}: CategoryDialogProps) {
  const createCategoryMutation = useCreateCategory();
  const updateCategoryMutation = useUpdateCategory();

  const isEditMode = mode === "edit";

  const title = isEditMode
    ? "Edit Category"
    : "Add Category";

  const description = isEditMode
    ? "Update category details and save your changes."
    : "Create a new category by filling the details below.";

  const formId = `category-form-${mode}`;

  const isSubmitting =
    createCategoryMutation.isLoading ||
    updateCategoryMutation.isLoading;

  const mutationError =
    createCategoryMutation.error ??
    updateCategoryMutation.error;

  const errorMessage =
    mutationError instanceof Error
      ? mutationError.message
      : null;

  const handleSubmit = async (
    values: CategoryFormValues
  ) => {
    try {
      if (isEditMode && category) {
        await updateCategoryMutation.updateCategoryMutationAsync({
          id: category.id,
          data: values,
        });
      } else {
        await createCategoryMutation.createCategoryMutationAsync(
          values
        );
      }

      onOpenChange(false);
    } catch {
      // Keep dialog open and show mutation error.
    }
  };

  return (
    <EntityDialog
      open={open}
      onOpenChange={onOpenChange}
      title={title}
      description={description}
    >
      <div className="space-y-5">
        <CategoryForm
          mode={mode}
          category={category}
          formId={formId}
          onSubmit={handleSubmit}
          errorMessage={errorMessage}
        />

        <EntityDialogFooter
          mode={mode}
          formId={formId}
          isLoading={isSubmitting}
          onCancel={() => onOpenChange(false)}
          createLabel="Create category"
          createLoadingLabel="Creating..."
          editLabel="Save changes"
          editLoadingLabel="Saving..."
        />
      </div>
    </EntityDialog>
  );
}
