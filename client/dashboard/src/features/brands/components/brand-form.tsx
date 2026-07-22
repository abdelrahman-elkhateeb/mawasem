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
import { Switch } from "@/components/ui/switch";
import { Textarea } from "@/components/ui/textarea";
import {
  brandFormDefaultValues,
  brandFormSchema,
  type BrandFormValues,
} from "../schema/brand-form-schema";
import type { BrandFormProps } from "./types";

export function BrandForm({
  mode,
  brand,
  formId,
  onSubmit,
  errorMessage,
}: BrandFormProps) {
  const form = useForm<BrandFormValues>({
    resolver: zodResolver(brandFormSchema),
    defaultValues:
      mode === "edit" && brand
        ? {
            nameAr: brand.nameAr,
            nameEn: brand.nameEn,
            descriptionAr: brand.descriptionAr,
            descriptionEn: brand.descriptionEn,
            logoUrl: brand.logoUrl,
            isActive: brand.isActive,
          }
        : brandFormDefaultValues,
  });

  useEffect(() => {
    if (mode === "edit" && brand) {
      form.reset({
        nameAr: brand.nameAr,
        nameEn: brand.nameEn,
        descriptionAr: brand.descriptionAr,
        descriptionEn: brand.descriptionEn,
        logoUrl: brand.logoUrl,
        isActive: brand.isActive,
      });

      return;
    }

    form.reset(brandFormDefaultValues);
  }, [brand, form, mode]);

  const handleFormSubmit = async (
    values: BrandFormValues
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
                  <Input placeholder="Brand name in Arabic" {...field} />
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
                  <Input placeholder="Brand name in English" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>

        <FormField
          control={form.control}
          name="descriptionAr"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Arabic Description</FormLabel>
              <FormControl>
                <Textarea
                  placeholder="Arabic brand description"
                  {...field}
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="descriptionEn"
          render={({ field }) => (
            <FormItem>
              <FormLabel>English Description</FormLabel>
              <FormControl>
                <Textarea
                  placeholder="English brand description"
                  {...field}
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="logoUrl"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Logo URL</FormLabel>
              <FormControl>
                <Input
                  placeholder="https://example.com/logo.png"
                  {...field}
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="isActive"
          render={({ field }) => (
            <FormItem className="flex flex-row items-center justify-between rounded-2xl border p-4">
              <div className="space-y-0.5">
                <FormLabel>Active</FormLabel>
                <p className="text-sm text-muted-foreground">
                  Toggle to control whether the brand is visible.
                </p>
              </div>

              <FormControl>
                <Switch
                  checked={field.value}
                  onCheckedChange={field.onChange}
                />
              </FormControl>
            </FormItem>
          )}
        />

        {errorMessage ? (
          <p className="text-sm text-destructive">{errorMessage}</p>
        ) : null}
      </form>
    </Form>
  );
}