import { z } from "zod";

export const changePasswordSchema = z
  .object({
    currentPassword: z
      .string()
      .min(8, "Password must contain at least 8 characters"),

    newPassword: z
      .string()
      .min(8, "Password must contain at least 8 characters"),

    confirmNewPassword: z.string(),
  })
  .refine(
    (data) => data.newPassword === data.confirmNewPassword,
    {
      path: ["confirmNewPassword"],
      message: "Passwords do not match",
    }
  );

export type ChangePasswordFormData = z.infer<
  typeof changePasswordSchema
>;