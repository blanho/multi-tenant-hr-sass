import { Chip, type ChipProps } from "@mui/material";

const colorMap: Record<string, ChipProps["color"]> = {
  Active: "success",
  Approved: "success",
  Delivered: "success",
  Read: "info",
  Clean: "success",
  Sent: "info",
  Professional: "primary",
  Enterprise: "secondary",
  Pending: "warning",
  PendingSetup: "warning",
  Normal: "default",
  Low: "default",
  Free: "default",
  Starter: "default",
  Info: "info",
  Warning: "warning",
  High: "warning",
  Suspended: "error",
  Rejected: "error",
  Cancelled: "error",
  Failed: "error",
  Critical: "error",
  Infected: "error",
  Error: "error",
};

interface StatusChipProps {
  status: string;
  size?: "small" | "medium";
}

export function StatusChip({ status, size = "small" }: Readonly<StatusChipProps>) {
  return (
    <Chip
      label={status}
      size={size}
      color={colorMap[status] ?? "default"}
      variant="outlined"
    />
  );
}
