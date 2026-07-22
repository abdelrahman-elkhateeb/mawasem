import { MoreHorizontal } from "lucide-react";
import { useState } from "react";

import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";

import { BrandDialog } from "./brand-dialog";
import { DeleteBrandDialog } from "./delete-brand-dialog";
import type { BrandActionsProps } from "./types";


export function BrandActions({
  brand,
}: BrandActionsProps) {
  const [isDeleteDialogOpen, setIsDeleteDialogOpen] =
    useState(false);
  const [isEditDialogOpen, setIsEditDialogOpen] =
    useState(false);

  return (
    <>
      <DropdownMenu>
        <DropdownMenuTrigger asChild>
          <Button
            variant="ghost"
            size="icon-sm"
            aria-label="Open actions"
          >
            <MoreHorizontal className="size-4" />
          </Button>
        </DropdownMenuTrigger>

        <DropdownMenuContent align="end">
          <DropdownMenuItem
            onClick={() =>
              setIsEditDialogOpen(true)
            }
          >
            Edit Brand
          </DropdownMenuItem>

          <DropdownMenuItem
            variant="destructive"
            onClick={() =>
              setIsDeleteDialogOpen(true)
            }
          >
            Delete Brand
          </DropdownMenuItem>
        </DropdownMenuContent>
      </DropdownMenu>

      <BrandDialog
        mode="edit"
        brand={brand}
        open={isEditDialogOpen}
        onOpenChange={setIsEditDialogOpen}
      />

      <DeleteBrandDialog
        brandId={brand.id}
        open={isDeleteDialogOpen}
        onOpenChange={setIsDeleteDialogOpen}
      />
    </>
  );
}
