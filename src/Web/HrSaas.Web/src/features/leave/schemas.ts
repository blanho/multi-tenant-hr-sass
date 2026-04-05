import { z } from "zod";

export const applyLeaveSchema = z.object({
  employeeId: z.string().min(1, "Employee is required"),
  tenantId: z.string().min(1, "Tenant is required"),
  type: z.enum(["Annual", "Sick", "Maternity", "Paternity", "Unpaid", "Emergency"]),
  startDate: z.string().min(1, "Start date is required"),
  endDate: z.string().min(1, "End date is required"),
  reason: z.string().min(1, "Reason is required").max(1000),
});

export type ApplyLeaveForm = z.infer<typeof applyLeaveSchema>;
