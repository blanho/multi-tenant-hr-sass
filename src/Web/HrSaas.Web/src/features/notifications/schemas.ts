import { z } from "zod";

export const sendNotificationSchema = z.object({
  userId: z.string().min(1, "User ID is required"),
  channel: z.enum(["Email", "Sms", "InApp", "Push", "Webhook", "Slack"]),
  category: z.enum(["System", "Leave", "Employee", "Billing", "Security", "Tenant", "General"]),
  priority: z.enum(["Low", "Normal", "High", "Critical"]),
  subject: z.string().min(1, "Subject is required"),
  body: z.string().min(1, "Body is required"),
});

export type SendNotificationForm = z.infer<typeof sendNotificationSchema>;

export const createTemplateSchema = z.object({
  name: z.string().min(1, "Name is required").max(200),
  slug: z.string().min(1, "Slug is required").max(100),
  description: z.string().max(500).optional(),
  channel: z.enum(["Email", "Sms", "InApp", "Push", "Webhook", "Slack"]),
  category: z.enum(["System", "Leave", "Employee", "Billing", "Security", "Tenant", "General"]),
  subjectTemplate: z.string().min(1, "Subject template is required"),
  bodyTemplate: z.string().min(1, "Body template is required"),
});

export type CreateTemplateForm = z.infer<typeof createTemplateSchema>;
