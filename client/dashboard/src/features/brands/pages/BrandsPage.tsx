import { useState } from "react";
import { useDebounce } from "use-debounce";

import { EntityTable } from "@/components/entity-table/entity-table";
import { EntityToolbar } from "@/components/entity-table/entity-toolbar";
import { EntityPagination } from "@/components/entity-table/entity-pagination";
import { normalizeArabic } from "@/lib/normalize-arabic";

import { useBrands } from "../hooks/use-brands";
import { brandColumns } from "@/components/entity-table/columns/brand-columns";


export function BrandsPage() {
  const [searchInput, setSearchInput] = useState("");

  const normalizedSearch =
    normalizeArabic(searchInput);

  const [debouncedSearch] = useDebounce(
    normalizedSearch,
    500
  );

  const [requestedPageNumber, setRequestedPageNumber] =
    useState(1);

  const {
    data,
    //  isLoading
  } = useBrands({
    search:
      debouncedSearch.length > 0
        ? debouncedSearch
        : undefined,
    pageNumber: requestedPageNumber,
    pageSize: 10,
  });

  const currentPage =
    data?.pageNumber ?? requestedPageNumber;

  const totalPages = data?.totalPages ?? 1;
  const totalCount = data?.totalCount ?? 0;

  const handleSearch = (value: string) => {
    setSearchInput(value);
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

  const handleAddBrand = () => {
    console.log("Open add dialog");
  };

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">
          Brands
        </h1>

        <p className="text-muted-foreground">
          Manage your brands.
        </p>
      </div>

      <EntityToolbar
        search={searchInput}
        onSearch={handleSearch}
        buttonText="Add Brand"
        onAdd={handleAddBrand}
      />

      <EntityTable
        columns={brandColumns}
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