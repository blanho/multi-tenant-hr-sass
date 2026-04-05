import type {
  NotificationCategory,
  NotificationChannel,
  NotificationPriority,
  DigestFrequency,
} from "@/types/shared";

export const CHANNELS: NotificationChannel[] = [
  "Email",
  "Sms",
  "InApp",
  "Push",
  "Webhook",
  "Slack",
];

export const CATEGORIES: NotificationCategory[] = [
  "System",
  "Leave",
  "Employee",
  "Billing",
  "Security",
  "Tenant",
  "General",
];

export const PRIORITIES: NotificationPriority[] = ["Low", "Normal", "High", "Critical"];

export const DIGEST_OPTIONS: DigestFrequency[] = [
  "None",
  "Daily",
  "Weekly",
  "BiWeekly",
  "Monthly",
];
