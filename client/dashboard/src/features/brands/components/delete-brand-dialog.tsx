import {
  AlertDialog,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog";
import { Button } from "@/components/ui/button";

import { useDeleteBrand } from "../hooks/use-delete-brand";
import type { DeleteBrandDialogProps } from "./types";


export function DeleteBrandDialog({
  brandId,
  open,
  onOpenChange,
}: DeleteBrandDialogProps) {
  const deleteBrandMutation = useDeleteBrand();

  const handleDelete = async () => {
    try {
      await deleteBrandMutation.mutateAsync(brandId);
      onOpenChange(false);
    } catch {
      // Error is shown in the dialog body.
    }
  };

  const errorMessage =
    deleteBrandMutation.error instanceof Error
      ? deleteBrandMutation.error.message
      : "Failed to delete brand. Please try again.";

  return (
    <AlertDialog
      open={open}
      onOpenChange={onOpenChange}
    >
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle>
            Delete brand
          </AlertDialogTitle>

          <AlertDialogDescription>
            Are you sure you want to delete this brand? This action cannot be undone.
          </AlertDialogDescription>

          {deleteBrandMutation.isError ? (
            <p className="text-sm text-destructive">
              {errorMessage}
            </p>
          ) : null}
        </AlertDialogHeader>

        <AlertDialogFooter>
          <AlertDialogCancel asChild>
            <Button
              variant="outline"
              disabled={deleteBrandMutation.isPending}
            >
              Cancel
            </Button>
          </AlertDialogCancel>

          <Button
            variant="destructive"
            onClick={handleDelete}
            disabled={deleteBrandMutation.isPending}
          >
            {deleteBrandMutation.isPending
              ? "Deleting..."
              : "Delete"}
          </Button>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}
