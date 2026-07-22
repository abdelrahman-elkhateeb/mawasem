import { z } from "zod";

export const brandFormSchema = z.object({
  nameAr: z
    .string()
    .trim()
    .min(1, "Arabic name is required."),
  nameEn: z
    .string()
    .trim()
    .min(1, "English name is required."),
  descriptionAr: z
    .string()
    .trim()
    .min(1, "Arabic description is required."),
  descriptionEn: z
    .string()
    .trim()
    .min(1, "English description is required."),
  logoUrl: z
    .string()
    .trim()
    .min(1, "Logo URL is required.")
    .url("Logo URL must be a valid URL."),
  isActive: z.boolean(),
});

export type BrandFormValues = z.infer<typeof brandFormSchema>;

export const brandFormDefaultValues: BrandFormValues = {
  nameAr: "",
  nameEn: "",
  descriptionAr: "",
  descriptionEn: "",
  logoUrl: "",
  isActive: true,
};