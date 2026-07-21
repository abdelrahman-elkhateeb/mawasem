import { Plus } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";

interface EntityToolbarProps {
  search: string;
  onSearch: (value: string) => void;

  buttonText: string;
  onAdd: () => void;
}

export function EntityToolbar({
  search,
  onSearch,
  buttonText,
  onAdd,
}: EntityToolbarProps) {
  return (
    <div className="flex items-center justify-between gap-4">
      <Input
        value={search}
        onChange={(e) => onSearch(e.target.value)}
        placeholder="Search..."
        className="max-w-sm"
      />

      <Button onClick={onAdd}>
        <Plus className="mr-2 h-4 w-4" />

        {buttonText}
      </Button>
    </div>
  );
}