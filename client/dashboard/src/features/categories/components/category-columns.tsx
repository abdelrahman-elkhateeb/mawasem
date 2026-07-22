import type { ColumnDef } from "@tanstack/react-table";
import { MoreHorizontal } from "lucide-react";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";

import type { Category } from "../types/category";
import type { CategoryColumnActions } from "./types";

export function createCategoryColumns({
  onEdit,
  onDelete,
  onRestore,
}: CategoryColumnActions): ColumnDef<Category>[] {
  return [
    {
      accessorKey: "nameAr",
      header: "Arabic Name",
    },
    {
      accessorKey: "nameEn",
      header: "English Name",
    },
    {
      accessorKey: "productCount",
      header: "Product Count",
    },
    {
      accessorKey: "isDeleted",
      header: "Status",
      cell: ({ row }) => {
        const isDeleted = row.original.isDeleted;

        return (
          <Badge
            variant={
              isDeleted
                ? "secondary"
                : "default"
            }
          >
            {isDeleted ? "Deleted" : "Active"}
          </Badge>
        );
      },
    },
    {
      id: "actions",
      header: "Actions",
      cell: ({ row }) => {
        const category = row.original;

        return (
          <div className="flex justify-end">
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
                    onEdit(category)
                  }
                >
                  Edit Category
                </DropdownMenuItem>

                {category.isDeleted ? (
                  <DropdownMenuItem
                    onClick={() =>
                      onRestore(category.id)
                    }
                  >
                    Restore Category
                  </DropdownMenuItem>
                ) : (
                  <DropdownMenuItem
                    variant="destructive"
                    onClick={() =>
                      onDelete(category.id)
                    }
                  >
                    Delete Category
                  </DropdownMenuItem>
                )}
              </DropdownMenuContent>
            </DropdownMenu>
          </div>
        );
      },
    },
  ];
}
