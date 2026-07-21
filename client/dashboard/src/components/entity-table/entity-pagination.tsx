interface EntityPaginationProps {
  page: number;
  totalPages: number;
  onPageChange: (
    page: number
  ) => void;
}

export function EntityPagination({
  page,
  totalPages,
  onPageChange,
}: EntityPaginationProps) {
  return (
    <div className="flex gap-2">
      <button
        disabled={page === 1}
        onClick={() =>
          onPageChange(page - 1)
        }
      >
        Previous
      </button>

      <span>
        {page} / {totalPages}
      </span>

      <button
        disabled={
          page === totalPages
        }
        onClick={() =>
          onPageChange(page + 1)
        }
      >
        Next
      </button>
    </div>
  );
}