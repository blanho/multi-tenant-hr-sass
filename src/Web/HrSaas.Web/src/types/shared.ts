export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export type NotificationChannel =
  | "InApp"
  | "Email"
  | "Sms"
  | "Push"
  | "Webhook"
  | "Slack";

export type NotificationCategory =
  | "System"
  | "Leave"
  | "Employee"
  | "Billing"
  | "Security"
  | "Tenant"
  | "General";

export type NotificationPriority = "Low" | "Normal" | "High" | "Critical";

export type NotificationStatus =
  | "Pending"
  | "Sent"
  | "Delivered"
  | "Read"
  | "Failed"
  | "Cancelled";

export type LeaveType =
  | "Annual"
  | "Sick"
  | "Maternity"
  | "Paternity"
  | "Unpaid"
  | "Emergency";

export type LeaveStatus = "Pending" | "Approved" | "Rejected" | "Cancelled";

export type TenantPlan = "Free" | "Starter" | "Professional" | "Enterprise";

export type TenantStatus = "Active" | "Suspended" | "PendingSetup";

export type BillingCycle = "Monthly" | "Annual";

export type FileCategory =
  | "General"
  | "Avatar"
  | "Document"
  | "Contract"
  | "Receipt"
  | "Resume"
  | "Policy"
  | "Report"
  | "Template"
  | "Attachment";

export type FileScanStatus = "Pending" | "Clean" | "Infected" | "Error";

export type AuditAction =
  | "Created"
  | "Updated"
  | "Deleted"
  | "Viewed"
  | "Login"
  | "Logout"
  | "PasswordChanged"
  | "RoleAssigned"
  | "RoleRemoved"
  | "PermissionGranted"
  | "PermissionRevoked"
  | "TenantCreated"
  | "TenantSuspended"
  | "TenantReinstated"
  | "PlanUpgraded"
  | "SubscriptionCreated"
  | "SubscriptionCancelled"
  | "LeaveApplied"
  | "LeaveApproved"
  | "LeaveRejected"
  | "LeaveCancelled";

export type AuditEntityType =
  | "User"
  | "Employee"
  | "Leave"
  | "Tenant"
  | "Role"
  | "Subscription"
  | "Notification"
  | "File";

export type AuditSeverity = "Info" | "Warning" | "Error";

export type DigestFrequency =
  | "None"
  | "Daily"
  | "Weekly"
  | "BiWeekly"
  | "Monthly";

export interface FeatureDto {
  name: string;
  isEnabled: boolean;
}
