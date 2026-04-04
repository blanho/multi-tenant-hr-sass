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

export interface UserDto {
  id: string;
  tenantId: string;
  email: string;
  roleId: string;
  roleName: string;
  permissions: string[];
  isActive: boolean;
  createdAt: string;
}

export interface AuthTokenDto {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: UserDto;
}

export interface EmployeeSummaryDto {
  id: string;
  name: string;
  department: string;
  position: string;
}

export interface EmployeeDto extends EmployeeSummaryDto {
  tenantId: string;
  email: string;
  createdAt: string;
  updatedAt: string | null;
}

export interface RoleDto {
  id: string;
  tenantId: string;
  name: string;
  isSystem: boolean;
  permissions: string[];
  createdAt: string;
}

export interface LeaveRequestDto {
  id: string;
  tenantId: string;
  employeeId: string;
  type: LeaveType;
  status: LeaveStatus;
  startDate: string;
  endDate: string;
  reason: string;
  rejectionNote: string | null;
  durationDays: number;
  createdAt: string;
}

export interface LeaveBalanceDto {
  id: string;
  employeeId: string;
  year: number;
  annualAllowance: number;
  sickAllowance: number;
  annualUsed: number;
  sickUsed: number;
  annualRemaining: number;
  sickRemaining: number;
}

export interface TenantDto {
  id: string;
  name: string;
  slug: string;
  contactEmail: string;
  plan: TenantPlan;
  status: TenantStatus;
  maxEmployees: number;
  createdAt: string;
}

export interface SubscriptionDto {
  id: string;
  tenantId: string;
  planName: string;
  status: string;
  billingCycle: BillingCycle;
  pricePerCycle: number;
  maxSeats: number;
  usedSeats: number;
  trialEndsAt: string | null;
  currentPeriodEnd: string | null;
  createdAt: string;
}

export interface NotificationSummaryDto {
  id: string;
  userId: string;
  tenantId: string;
  channel: NotificationChannel;
  category: NotificationCategory;
  priority: NotificationPriority;
  subject: string;
  body: string;
  status: NotificationStatus;
  createdAt: string;
  readAt: string | null;
  deliveredAt: string | null;
  correlationId: string | null;
}

export interface NotificationDetailDto extends NotificationSummaryDto {
  scheduledFor: string | null;
  expiresAt: string | null;
  metadata: string | null;
  templateId: string | null;
  retryCount: number;
  maxRetries: number;
  groupKey: string | null;
  updatedAt: string | null;
}

export interface NotificationStatsDto {
  totalCount: number;
  readCount: number;
  unreadCount: number;
  sentCount: number;
  deliveredCount: number;
  failedCount: number;
}

export interface FeatureDto {
  name: string;
  isEnabled: boolean;
}

export interface FileSummaryDto {
  id: string;
  fileName: string;
  contentType: string;
  fileSize: number;
  category: FileCategory;
  scanStatus: FileScanStatus;
  description: string | null;
  tags: string | null;
  uploadedBy: string;
  entityId: string | null;
  createdAt: string;
}

export interface FileDetailDto extends FileSummaryDto {
  storagePath: string;
  entityType: string | null;
  updatedAt: string | null;
}

export interface FileUploadResult {
  id: string;
  fileName: string;
  fileSize: number;
  contentType: string;
}

export interface FileUrlDto {
  url: string;
  expiresAt: string;
}

export interface AuditLogSummaryDto {
  id: string;
  tenantId: string;
  userId: string | null;
  userEmail: string | null;
  action: AuditAction;
  entityType: AuditEntityType;
  entityId: string;
  description: string;
  ipAddress: string | null;
  severity: AuditSeverity;
  correlationId: string | null;
  isSystemGenerated: boolean;
  tags: string | null;
  userAgent: string | null;
  durationMs: number;
  createdAt: string;
}

export interface AuditLogDetailDto extends AuditLogSummaryDto {
  oldValues: string | null;
  newValues: string | null;
  additionalData: string | null;
  requestPath: string | null;
  requestMethod: string | null;
}

export interface NotificationPreferencesDto {
  id: string;
  userId: string;
  tenantId: string;
  enabledChannels: NotificationChannel[];
  mutedCategories: NotificationCategory[];
  emailEnabled: boolean;
  digestFrequency: DigestFrequency;
  quietHoursStart: string | null;
  quietHoursEnd: string | null;
  timezone: string | null;
}

export interface NotificationTemplateDto {
  id: string;
  name: string;
  description: string;
  channel: NotificationChannel;
  category: NotificationCategory;
  subjectTemplate: string;
  bodyTemplate: string;
  isActive: boolean;
  metadata: string | null;
}

export interface LoginPayload {
  tenantId: string;
  email: string;
  password: string;
}

export interface RegisterPayload {
  tenantId: string;
  email: string;
  password: string;
  fullName?: string;
}

export interface CreateEmployeePayload {
  name: string;
  department: string;
  position: string;
  email: string;
}

export interface UpdateEmployeePayload {
  name: string;
  department: string;
  position: string;
}

export interface ApplyLeavePayload {
  tenantId: string;
  employeeId: string;
  type: LeaveType;
  startDate: string;
  endDate: string;
  reason: string;
}

export interface CreateTenantPayload {
  name: string;
  slug: string;
  contactEmail: string;
  plan?: TenantPlan;
}

export interface UpdateTenantPayload {
  name: string;
  contactEmail: string;
}

export interface CreateRolePayload {
  name: string;
  permissions?: string[];
}

export interface UpdateRolePermissionsPayload {
  permissions: string[];
}

export interface AssignRolePayload {
  userId: string;
  roleId: string;
}

export interface CreateNotificationPayload {
  userId: string;
  channel: NotificationChannel;
  category: NotificationCategory;
  priority: NotificationPriority;
  subject: string;
  body: string;
  recipient?: string;
  groupKey?: string;
  scheduledFor?: string;
  metadata?: string;
  templateId?: string;
  expiresAt?: string;
}

export interface UpdateNotificationPreferencesPayload {
  enabledChannels: NotificationChannel[];
  mutedCategories: NotificationCategory[];
  emailEnabled: boolean;
  digestFrequency: DigestFrequency;
  quietHoursStart?: string;
  quietHoursEnd?: string;
  timezone?: string;
}

export interface CreateNotificationTemplatePayload {
  name: string;
  description: string;
  channel: NotificationChannel;
  category: NotificationCategory;
  subjectTemplate: string;
  bodyTemplate: string;
  metadata?: string;
  variables?: string;
}

export interface UpdateSubscriptionPayload {
  amount: number;
  billingCycle: BillingCycle;
  promoCode?: string;
}

export interface CancelSubscriptionPayload {
  subscriptionId: string;
  reason: string;
}
