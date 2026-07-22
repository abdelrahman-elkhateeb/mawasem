import { z } from "zod";

export const categoryFormSchema = z.object({
  nameAr: z
    .string()
    .trim()
    .min(2, "Arabic name must be at least 2 characters."),
  nameEn: z
    .string()
    .trim()
    .min(2, "English name must be at least 2 characters."),
});

export type CategoryFormValues =
  z.infer<typeof categoryFormSchema>;

export const categoryFormDefaultValues: CategoryFormValues =
  {
    nameAr: "",
    nameEn: "",
  };
