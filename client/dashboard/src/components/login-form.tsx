"use client";

import { Button } from "@/components/ui/button";
import {
  Field,
  FieldDescription,
  FieldGroup,
  FieldLabel,
} from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { cn } from "@/lib/utils";
import { BookOpen } from "lucide-react";
import { Link } from "react-router-dom";

export function LoginForm({
  className,
  ...props
}: React.ComponentProps<"div">) {
  return (
    <div className={cn("flex flex-col gap-6", className)} {...props}>
      <form>
        <FieldGroup>
          <div className="flex flex-col items-center gap-2 text-center">
            <a
              href="#"
              className="flex flex-col items-center gap-2 font-medium"
            >
              <div className="flex size-12 items-center justify-center rounded-xl bg-primary text-primary-foreground">
                <BookOpen className="size-6" />
              </div>

              <span className="text-xl font-bold">Mawasem</span>
            </a>

            <h1 className="text-xl font-bold">
              Welcome back to Mawasem
            </h1>

            <FieldDescription>
              Don&apos;t have an account? <Link to="/auth/signup">Sign up</Link>
            </FieldDescription>
          </div>

          <Field>
            <FieldLabel htmlFor="email">Email</FieldLabel>

            <Input
              id="email"
              type="email"
              placeholder="m@example.com"
              required
            />
          </Field>

          <Field>
            <FieldLabel htmlFor="password">Password</FieldLabel>

            <Input
              id="password"
              type="password"
              placeholder="**********"
              required
            />
          </Field>

          <Field>
            <Button type="submit" className="w-full">
              Login
            </Button>
          </Field>
        </FieldGroup>
      </form>
    </div>
  );
}