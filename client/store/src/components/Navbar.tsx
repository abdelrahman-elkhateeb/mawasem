import {
  Heart,
  Menu,
  Search,
  ShoppingCart,
  UserRound,
} from "lucide-react";
import { useState, type FormEvent } from "react";
import { Link, NavLink, useNavigate } from "react-router-dom";

import { ModeToggle } from "@/components/mode-toggle";
import { Badge } from "@/components/ui/badge";
import { Button, buttonVariants } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Sheet,
  SheetClose,
  SheetContent,
  SheetDescription,
  SheetHeader,
  SheetTitle,
  SheetTrigger,
} from "@/components/ui/sheet";
import { cn } from "@/lib/utils";

const navigationLinks = [
  {
    label: "Home",
    path: "/",
  },
  {
    label: "Products",
    path: "/products",
  },
  {
    label: "Back to School",
    path: "/seasons/back-to-school",
  },
  {
    label: "Summer",
    path: "/seasons/summer",
  },
  {
    label: "Winter",
    path: "/seasons/winter",
  },
];

interface StoreNavbarProps {
  cartCount?: number;
  wishlistCount?: number;
}

export default function StoreNavbar({
  cartCount = 0,
  wishlistCount = 0,
}: StoreNavbarProps) {
  const [searchQuery, setSearchQuery] = useState("");

  const navigate = useNavigate();

  function handleSearch(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    const trimmedQuery = searchQuery.trim();

    if (!trimmedQuery) return;

    navigate(
      `/products?search=${encodeURIComponent(trimmedQuery)}`
    );
  }

  return (
    <header className="sticky top-0 z-50 border-b bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/80">
      <div className="container mx-auto flex h-16 items-center gap-4 px-4 lg:px-6">
        {/* Mobile navigation */}
        <Sheet>
          <SheetTrigger asChild>
            <Button
              type="button"
              variant="ghost"
              size="icon"
              className="lg:hidden"
            >
              <Menu className="size-5" />

              <span className="sr-only">
                Open navigation
              </span>
            </Button>
          </SheetTrigger>

          <SheetContent side="left">
            <SheetHeader className="text-left">
              <SheetTitle className="text-2xl font-bold text-primary">
                Mawasem
              </SheetTitle>

              <SheetDescription>
                Shop products for every season and occasion.
              </SheetDescription>
            </SheetHeader>

            <nav className="mt-8 flex flex-col gap-2">
              {navigationLinks.map((link) => (
                <SheetClose asChild key={link.path}>
                  <NavLink
                    to={link.path}
                    end={link.path === "/"}
                    className={({ isActive }) =>
                      cn(
                        buttonVariants({
                          variant: isActive
                            ? "secondary"
                            : "ghost",
                        }),
                        "w-full justify-start"
                      )
                    }
                  >
                    {link.label}
                  </NavLink>
                </SheetClose>
              ))}
            </nav>

            <div className="mt-8 border-t pt-6">
              <SheetClose asChild>
                <Link
                  to="/auth/login"
                  className={cn(
                    buttonVariants(),
                    "w-full"
                  )}
                >
                  <UserRound className="size-4" />
                  Log in
                </Link>
              </SheetClose>
            </div>
          </SheetContent>
        </Sheet>

        {/* Logo */}
        <Link
          to="/"
          className="shrink-0 text-2xl font-bold tracking-tight text-primary"
        >
          Mawasem
        </Link>

        {/* Desktop navigation */}
        <nav className="hidden items-center gap-1 lg:flex">
          {navigationLinks.map((link) => (
            <NavLink
              key={link.path}
              to={link.path}
              end={link.path === "/"}
              className={({ isActive }) =>
                cn(
                  buttonVariants({
                    variant: isActive
                      ? "secondary"
                      : "ghost",
                    size: "sm",
                  }),
                  !isActive && "text-muted-foreground"
                )
              }
            >
              {link.label}
            </NavLink>
          ))}
        </nav>

        {/* Search */}
        <form
          onSubmit={handleSearch}
          className="relative mx-auto hidden max-w-md flex-1 md:block"
        >
          <Search className="absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />

          <Input
            type="search"
            value={searchQuery}
            onChange={(event) =>
              setSearchQuery(event.target.value)
            }
            placeholder="Search for products..."
            className="pl-9"
          />
        </form>

        {/* Navbar actions */}
        <div className="ml-auto flex items-center gap-1">
          {/* Mobile search */}
          <Button
            type="button"
            variant="ghost"
            size="icon"
            className="md:hidden"
            onClick={() => navigate("/products")}
          >
            <Search className="size-5" />
            <span className="sr-only">Search</span>
          </Button>

          {/* Shadcn mode toggle */}
          <ModeToggle />

          {/* Wishlist */}
          <Button
            variant="ghost"
            size="icon"
            asChild
            className="relative"
          >
            <Link to="/wishlist">
              <Heart className="size-5" />

              {wishlistCount > 0 && (
                <Badge className="absolute -right-1 -top-1 flex size-5 items-center justify-center rounded-full p-0 text-[10px]">
                  {wishlistCount > 99
                    ? "99+"
                    : wishlistCount}
                </Badge>
              )}

              <span className="sr-only">
                Wishlist
              </span>
            </Link>
          </Button>

          {/* Shopping cart */}
          <Button
            variant="ghost"
            size="icon"
            asChild
            className="relative"
          >
            <Link to="/cart">
              <ShoppingCart className="size-5" />

              {cartCount > 0 && (
                <Badge className="absolute -right-1 -top-1 flex size-5 items-center justify-center rounded-full p-0 text-[10px]">
                  {cartCount > 99
                    ? "99+"
                    : cartCount}
                </Badge>
              )}

              <span className="sr-only">
                Shopping cart
              </span>
            </Link>
          </Button>

          {/* Login */}
          <Button
            variant="outline"
            size="sm"
            asChild
            className="hidden sm:flex"
          >
            <Link to="/auth/login">
              <UserRound className="size-4" />
              Log in
            </Link>
          </Button>
        </div>
      </div>
    </header>
  );
}
