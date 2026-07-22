import { useEffect } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";

import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";

import {
  categoryFormDefaultValues,
  categoryFormSchema,
  type CategoryFormValues,
} from "../schema/category-form-schema";
import type { CategoryFormProps } from "./types";

export function CategoryForm({
  mode,
  category,
  formId,
  errorMessage,
  onSubmit,
}: CategoryFormProps) {
  const form = useForm<CategoryFormValues>({
    resolver: zodResolver(categoryFormSchema),
    defaultValues:
      mode === "edit" && category
        ? {
            nameAr: category.nameAr,
            nameEn: category.nameEn,
          }
        : categoryFormDefaultValues,
  });

  useEffect(() => {
    if (mode === "edit" && category) {
      form.reset({
        nameAr: category.nameAr,
        nameEn: category.nameEn,
      });

      return;
    }

    form.reset(categoryFormDefaultValues);
  }, [category, form, mode]);

  const handleFormSubmit = async (
    values: CategoryFormValues
  ) => {
    await onSubmit(values);
  };

  return (
    <Form {...form}>
      <form
        id={formId}
        onSubmit={form.handleSubmit(handleFormSubmit)}
        className="space-y-5"
      >
        <div className="grid gap-4 md:grid-cols-2">
          <FormField
            control={form.control}
            name="nameAr"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Arabic Name</FormLabel>
                <FormControl>
                  <Input
                    placeholder="Category name in Arabic"
                    {...field}
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="nameEn"
            render={({ field }) => (
              <FormItem>
                <FormLabel>English Name</FormLabel>
                <FormControl>
                  <Input
                    placeholder="Category name in English"
                    {...field}
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>

        {errorMessage ? (
          <p className="text-sm text-destructive">
            {errorMessage}
          </p>
        ) : null}
      </form>
    </Form>
  );
}
