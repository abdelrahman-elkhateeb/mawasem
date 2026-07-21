import {
  Building2,
  FolderKanban,
  LayoutGrid,
} from "lucide-react";

export const data = [
  {
    key: "catalog",
    url: "#",

    items: [
      {
        key: "brands",
        url: "/brands",
        icon: Building2,
      },
      {
        key: "categories",
        url: "/categories",
        icon: LayoutGrid,
      },
      {
        key: "collections",
        url: "/collections",
        icon: FolderKanban,
      },
    ],
  },
];