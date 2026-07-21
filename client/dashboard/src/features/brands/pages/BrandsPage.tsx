import { useState } from "react";

import { EntityTable } from "@/components/entity-table/entity-table";
import { EntityToolbar } from "@/components/entity-table/entity-toolbar";
import { EntityPagination } from "@/components/entity-table/entity-pagination";

import { useBrands } from "../hooks/use-brands";
import { brandColumns } from "@/components/entity-table/columns/brand-columns";


export function BrandsPage() {
  const [search, setSearch] = useState("");

  const [pageNumber, setPageNumber] = useState(1);

  const {
    data,
    //  isLoading
  } = useBrands({
    search,
    pageNumber,
    pageSize: 10,
  });

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
        search={search}
        onSearch={setSearch}
        buttonText="Add Brand"
        onAdd={handleAddBrand}
      />

      <EntityTable
        columns={brandColumns}
        data={data?.items ?? []}
      // isLoading={isLoading}
      />

      <EntityPagination
        page={pageNumber}
        totalPages={data?.totalPages ?? 1}
        onPageChange={setPageNumber}
      />
    </div>
  );
}