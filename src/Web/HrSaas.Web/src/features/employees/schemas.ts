import { z } from "zod";

export const createEmployeeSchema = z.object({
  name: z.string().min(2, "Name is required").max(200),
  department: z.string().min(2, "Department is required").max(100),
  position: z.string().min(2, "Position is required").max(100),
  email: z.email("Valid email is required"),
});

export type CreateEmployeeForm = z.infer<typeof createEmployeeSchema>;

export const editEmployeeSchema = z.object({
  name: z.string().min(2, "Name is required").max(200),
  department: z.string().min(2, "Department is required").max(100),
  position: z.string().min(2, "Position is required").max(100),
});

export type EditEmployeeForm = z.infer<typeof editEmployeeSchema>;
