import type { AuditAction, AuditEntityType, AuditSeverity } from "@/types/shared";

export const ACTIONS: AuditAction[] = [
  "Created",
  "Updated",
  "Deleted",
  "Viewed",
  "Login",
  "Logout",
  "PasswordChanged",
  "RoleAssigned",
  "RoleRemoved",
  "PermissionGranted",
  "PermissionRevoked",
  "TenantCreated",
  "TenantSuspended",
  "TenantReinstated",
  "PlanUpgraded",
  "SubscriptionCreated",
  "SubscriptionCancelled",
  "LeaveApplied",
  "LeaveApproved",
  "LeaveRejected",
  "LeaveCancelled",
];

export const ENTITY_TYPES: AuditEntityType[] = [
  "User",
  "Employee",
  "Leave",
  "Tenant",
  "Role",
  "Subscription",
  "Notification",
  "File",
];

export const SEVERITIES: AuditSeverity[] = ["Info", "Warning", "Error"];
