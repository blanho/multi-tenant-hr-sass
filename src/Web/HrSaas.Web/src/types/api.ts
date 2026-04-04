export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages?: number;
  hasNextPage?: boolean;
  hasPreviousPage?: boolean;
}

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
  updatedAt?: string | null;
}

export interface LeaveRequestDto {
  id: string;
  tenantId: string;
  employeeId: string;
  type: string;
  status: string;
  startDate: string;
  endDate: string;
  reason: string;
  rejectionNote?: string | null;
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
  plan: string;
  status: string;
  maxEmployees: number;
  createdAt: string;
}

export interface SubscriptionDto {
  id: string;
  tenantId: string;
  planName: string;
  status: string;
  billingCycle: string;
  pricePerCycle: number;
  maxSeats: number;
  usedSeats: number;
  trialEndsAt?: string | null;
  currentPeriodEnd?: string | null;
  createdAt: string;
}

export interface NotificationDto {
  id: string;
  userId: string;
  channel: string;
  category: string;
  priority: string;
  subject: string;
  body: string;
  status: string;
  createdAt: string;
  readAt?: string | null;
  deliveredAt?: string | null;
  correlationId?: string | null;
}

export interface NotificationPagedResult {
  items: NotificationDto[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasNext: boolean;
  hasPrevious: boolean;
}

export interface LoginPayload {
  tenantId: string;
  email: string;
  password: string;
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
  type: "Annual" | "Sick" | "Unpaid";
  startDate: string;
  endDate: string;
  reason: string;
}

export interface CreateTenantPayload {
  name: string;
  slug: string;
  contactEmail: string;
  plan: "Free" | "Starter" | "Professional" | "Enterprise";
}
