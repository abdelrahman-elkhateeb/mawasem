
import { useState } from "react";

import { EntityPagination } from "@/components/entity-table/entity-pagination";
import { EntityTable } from "@/components/entity-table/entity-table";
import { EntityToolbar } from "@/components/entity-table/entity-toolbar";
import { Label } from "@/components/ui/label";
import { Switch } from "@/components/ui/switch";
import { CategoryDialog } from "../components/category-dialog";
import {
  createCategoryColumns,
} from "../components/category-columns";
import { useCategories } from "../hooks/use-categories";
import { useDeleteCategory } from "../hooks/use-delete-category";
import { useRestoreCategory } from "../hooks/use-restore-category";
import type { Category } from "../types/category";

export default function CategoriesPage() {
  const [search, setSearch] = useState("");
  const [includeDeleted, setIncludeDeleted] =
    useState(false);
  const [requestedPageNumber, setRequestedPageNumber] =
    useState(1);
  const [isCreateDialogOpen, setIsCreateDialogOpen] =
    useState(false);
  const [editingCategory, setEditingCategory] =
    useState<Category | undefined>(undefined);

  const {
    data,
    isLoading,
    error,
  } = useCategories({
    search,
    includeDeleted,
    pageNumber: requestedPageNumber,
    pageSize: 10,
  });

  const {
    deleteCategoryMutationAsync,
  } = useDeleteCategory();
  const {
    restoreCategoryMutationAsync,
  } = useRestoreCategory();

  const currentPage =
    data?.pageNumber ?? requestedPageNumber;

  const totalPages =
    data?.totalPages ?? 1;

  const totalCount =
    data?.totalCount ?? 0;

  const handleSearch = (value: string) => {
    setSearch(value);
    setRequestedPageNumber(1);
  };

  const handleIncludeDeletedChange = (
    value: boolean
  ) => {
    setIncludeDeleted(value);
    setRequestedPageNumber(1);
  };

  const handlePageChange = (
    nextPage: number
  ) => {
    if (
      nextPage < 1 ||
      nextPage > totalPages ||
      nextPage === currentPage
    ) {
      return;
    }

    setRequestedPageNumber(nextPage);
  };

  const handleAddCategory = () => {
    setIsCreateDialogOpen(true);
  };

  const handleEditCategory = (
    category: Category
  ) => {
    setEditingCategory(category);
  };

  const handleDeleteCategory = async (
    categoryId: number
  ) => {
    const isConfirmed = window.confirm(
      "Are you sure you want to delete this category?"
    );

    if (!isConfirmed) {
      return;
    }

    try {
      await deleteCategoryMutationAsync(categoryId);
    } catch {
      // Query and mutation errors are surfaced by React Query.
    }
  };

  const handleRestoreCategory = async (
    categoryId: number
  ) => {
    try {
      await restoreCategoryMutationAsync(categoryId);
    } catch {
      // Query and mutation errors are surfaced by React Query.
    }
  };

  const categoryColumns =
    createCategoryColumns({
      onEdit: (category) => {
        void handleEditCategory(category);
      },
      onDelete: (categoryId) => {
        void handleDeleteCategory(categoryId);
      },
      onRestore: (categoryId) => {
        void handleRestoreCategory(categoryId);
      },
    });

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">
          Categories
        </h1>

        <p className="text-muted-foreground">
          Manage your categories.
        </p>
      </div>

      <EntityToolbar
        search={search}
        onSearch={handleSearch}
        buttonText="Add Category"
        onAdd={handleAddCategory}
      />

      <div className="flex items-center gap-2">
        <Switch
          id="include-deleted-categories"
          checked={includeDeleted}
          onCheckedChange={
            handleIncludeDeletedChange
          }
        />

        <Label htmlFor="include-deleted-categories">
          Include deleted
        </Label>
      </div>

      <EntityTable
        columns={categoryColumns}
        data={data?.items ?? []}
      />

      {isLoading ? (
        <p className="text-sm text-muted-foreground">
          Loading categories...
        </p>
      ) : null}

      {error instanceof Error ? (
        <p className="text-sm text-destructive">
          {error.message}
        </p>
      ) : null}

      <EntityPagination
        totalCount={totalCount}
        page={currentPage}
        totalPages={totalPages}
        onPageChange={handlePageChange}
      />

      <CategoryDialog
        mode="create"
        open={isCreateDialogOpen}
        onOpenChange={setIsCreateDialogOpen}
      />

      <CategoryDialog
        mode="edit"
        category={editingCategory}
        open={Boolean(editingCategory)}
        onOpenChange={(open) => {
          if (!open) {
            setEditingCategory(undefined);
          }
        }}
      />
    </div>
  );
}
