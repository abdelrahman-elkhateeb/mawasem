import { Button } from "@/components/ui/button";
import {
  Field,
  FieldDescription,
  FieldError,
  FieldGroup,
  FieldLabel,
} from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { useChangePassword } from "@/features/auth/hooks/use-change-password";
import {
  changePasswordSchema,
  type ChangePasswordFormData,
} from "@/features/auth/schema/change-password-schema";
import { cn } from "@/lib/utils";
import { zodResolver } from "@hookform/resolvers/zod";
import { BookOpen } from "lucide-react";
import { useForm } from "react-hook-form";

export function ChangePasswordForm({
  className,
  ...props
}: React.ComponentProps<"div">) {
  const { changePassword, isPending } = useChangePassword();

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<ChangePasswordFormData>({
    resolver: zodResolver(changePasswordSchema),
    defaultValues: {
      currentPassword: "",
      newPassword: "",
      confirmNewPassword: "",
    },
  });

  function onSubmit(data: ChangePasswordFormData) {
    console.log(data);

    // changePassword(data);
  }

  return (
    <div className={cn("flex flex-col gap-6", className)} {...props}>
      <form onSubmit={handleSubmit(onSubmit)}>
        <FieldGroup>
          <div className="flex flex-col items-center gap-2 text-center">
            <div className="flex size-12 items-center justify-center rounded-xl bg-primary text-primary-foreground">
              <BookOpen className="size-6" />
            </div>

            <span className="text-xl font-bold">Mawasem</span>

            <h1 className="text-xl font-bold">
              Change your password
            </h1>

            <FieldDescription>
              You must change your password before continuing.
            </FieldDescription>
          </div>

          <Field>
            <FieldLabel htmlFor="currentPassword">
              Current password
            </FieldLabel>

            <Input
              id="currentPassword"
              type="password"
              placeholder="**********"
              {...register("currentPassword")}
            />

            {errors.currentPassword && (
              <FieldError>
                {errors.currentPassword.message}
              </FieldError>
            )}
          </Field>

          <Field>
            <FieldLabel htmlFor="newPassword">
              New password
            </FieldLabel>

            <Input
              id="newPassword"
              type="password"
              placeholder="**********"
              {...register("newPassword")}
            />

            {errors.newPassword && (
              <FieldError>
                {errors.newPassword.message}
              </FieldError>
            )}
          </Field>

          <Field>
            <FieldLabel htmlFor="confirmNewPassword">
              Confirm password
            </FieldLabel>

            <Input
              id="confirmNewPassword"
              type="password"
              placeholder="**********"
              {...register("confirmNewPassword")}
            />

            {errors.confirmNewPassword && (
              <FieldError>
                {errors.confirmNewPassword.message}
              </FieldError>
            )}
          </Field>

          <Field>
            <Button
              type="submit"
              className="w-full"
            // disabled={isPending}
            >
              {/* {isPending ? "Loading..." : "Change password"} */}
              Change password
            </Button>
          </Field>
        </FieldGroup>
      </form>
    </div>
  );
}