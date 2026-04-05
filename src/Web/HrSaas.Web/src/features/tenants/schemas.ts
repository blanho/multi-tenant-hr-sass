import { z } from "zod";

export const createTenantSchema = z.object({
  name: z.string().min(2).max(200),
  slug: z
    .string()
    .min(2)
    .max(100)
    .regex(/^[a-z0-9-]+$/, "Lowercase letters, numbers, hyphens only"),
  contactEmail: z.email(),
  plan: z.enum(["Free", "Starter", "Professional", "Enterprise"]),
});

export type CreateTenantForm = z.infer<typeof createTenantSchema>;

export const editTenantSchema = z.object({
  name: z.string().min(2).max(200),
  contactEmail: z.email(),
});

export type EditTenantForm = z.infer<typeof editTenantSchema>;
