
import { useState } from "react";
import { useDebounce } from "use-debounce";

import { categoryColumns } from "@/components/entity-table/columns/category-columns";
import { EntityPagination } from "@/components/entity-table/entity-pagination";
import { EntityTable } from "@/components/entity-table/entity-table";
import { EntityToolbar } from "@/components/entity-table/entity-toolbar";
import { Label } from "@/components/ui/label";
import { Switch } from "@/components/ui/switch";
import { normalizeArabic } from "@/lib/normalize-arabic";

import { useCategories } from "../hooks/use-categories";

export default function CategoriesPage() {
  const [search, setSearch] = useState("");
  const [includeDeleted, setIncludeDeleted] =
    useState(false);
  const [requestedPageNumber, setRequestedPageNumber] =
    useState(1);

  const normalizedSearch =
    normalizeArabic(search);

  const [debouncedSearch] = useDebounce(
    normalizedSearch,
    500
  );

  const {
    data,
    // isLoading,
  } = useCategories({
    search:
      debouncedSearch.length > 0
        ? debouncedSearch
        : undefined,
    includeDeleted,
    pageNumber: requestedPageNumber,
    pageSize: 10,
  });

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
    checked: boolean
  ) => {
    setIncludeDeleted(checked);
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
    // Category create dialog is not wired yet.
  };

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
      // isLoading={isLoading}
      />

      <EntityPagination
        totalCount={totalCount}
        page={currentPage}
        totalPages={totalPages}
        onPageChange={handlePageChange}
      />
    </div>
  );
}
