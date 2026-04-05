import type { FileCategory, FileScanStatus } from "@/types/shared";

export const FILE_CATEGORIES: FileCategory[] = [
  "General",
  "Avatar",
  "Document",
  "Contract",
  "Receipt",
  "Resume",
  "Policy",
  "Report",
  "Template",
  "Attachment",
];

export const SCAN_STATUSES: FileScanStatus[] = ["Pending", "Clean", "Infected", "Error"];
