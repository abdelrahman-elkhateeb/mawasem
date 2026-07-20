"use client";

import { Button } from "@/components/ui/button";
import {
  Field,
  FieldDescription,
  FieldError,
  FieldGroup,
  FieldLabel,
} from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { useLogin } from "@/features/auth/hooks/use-login";
import { loginSchema, type LoginFormData } from "@/features/auth/schema/login-schema";
import { cn } from "@/lib/utils";
import { zodResolver } from "@hookform/resolvers/zod";
import { BookOpen } from "lucide-react";
import { useForm } from "react-hook-form";
import { Link, useNavigate } from "react-router-dom";

export function LoginForm({
  className,
  ...props
}: React.ComponentProps<"div">) {
  const navigate = useNavigate();
  const { isUserLoading, loginUser, error } = useLogin();

  const { register, handleSubmit, formState: { errors } } = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema),
    defaultValues: {
      email: "",
      password: "",
    },
  })

  function onSubmit(data: LoginFormData) {
    loginUser(data, {
      onSuccess: (response) => {
        if (response.user.mustChangePassword) {
          navigate("/auth/change-password", {
            replace: true,
          });

          return;
        }

        navigate("/", {
          replace: true,
        });
      },
    });
  }

  return (
    <div className={cn("flex flex-col gap-6", className)} {...props}>
      <form onSubmit={handleSubmit(onSubmit)}>
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
              {...register("email")}
            />
            {errors.email && (
              <FieldError>{errors.email.message}</FieldError>
            )}
          </Field>

          <Field>
            <FieldLabel htmlFor="password">Password</FieldLabel>

            <Input
              id="password"
              type="password"
              placeholder="**********"
              required
              {...register("password")}
            />
          </Field>

          <Field>
            <Button type="submit" className="w-full" disabled={isUserLoading}>
              {isUserLoading ? "loading..." : "Login"}
            </Button>
          </Field>
        </FieldGroup>
      </form>
    </div>
  );
}