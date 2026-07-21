import type { ColumnDef } from "@tanstack/react-table";

export interface EntityTableProps<TData, TValue> {
  columns: ColumnDef<TData, TValue>[];
  data: TData[];
}


export interface EntityToolbarProps {
  search: string;
  onSearch: (value: string) => void;

  buttonText: string;
  onAdd: () => void;
}

export interface EntityPaginationProps {
  totalCount: number;
  page: number;
  totalPages: number;
  onPageChange: (
    page: number
  ) => void;
}
