import { z } from "zod";

export const createRoleSchema = z.object({
  name: z.string().min(2, "Role name is required").max(100),
  permissions: z.array(z.string()).min(0),
});

export type CreateRoleForm = z.infer<typeof createRoleSchema>;
