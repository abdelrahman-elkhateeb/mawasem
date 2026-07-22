import type { ColumnDef } from "@tanstack/react-table";

import { Badge } from "@/components/ui/badge";
import type { Category } from "@/features/categories/types/category";

export const categoryColumns: ColumnDef<Category>[] = [
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

		cell: () => (
			<div className="flex justify-end">
				<span className="text-sm text-muted-foreground">
					-
				</span>
			</div>
		),
	},
];
