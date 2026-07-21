import { Button } from "@/components/ui/button";

interface EntityPaginationProps {
  totalCount: number;
  page: number;
  totalPages: number;
  onPageChange: (
    page: number
  ) => void;
}

export function EntityPagination({
  totalCount,
  page,
  totalPages,
  onPageChange,
}: EntityPaginationProps) {
  const safeTotalPages =
    totalPages > 0 ? totalPages : 1;

  return (
    <div className="flex items-center justify-between px-2">
      <div className="flex-1 text-sm text-muted-foreground">
        {totalCount} row(s)
      </div>

      <div className="flex items-center space-x-6 lg:space-x-8">
        <div className="text-sm font-medium">
          Page {page} of {safeTotalPages}
        </div>

        <div className="flex items-center space-x-2">
          <Button
            variant="outline"
            size="sm"
            disabled={page <= 1}
            onClick={() =>
              onPageChange(page - 1)
            }
          >
            Previous
          </Button>

          <Button
            variant="outline"
            size="sm"
            disabled={page >= safeTotalPages}
            onClick={() =>
              onPageChange(page + 1)
            }
          >
            Next
          </Button>
        </div>
      </div>
    </div>
  );
}
