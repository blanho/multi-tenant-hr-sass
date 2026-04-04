import { http } from "./http";
import type {
  ApplyLeavePayload,
  AuthTokenDto,
  CreateEmployeePayload,
  CreateTenantPayload,
  EmployeeDto,
  EmployeeSummaryDto,
  LeaveBalanceDto,
  LeaveRequestDto,
  LoginPayload,
  NotificationPagedResult,
  PagedResult,
  SubscriptionDto,
  TenantDto,
  UpdateEmployeePayload,
} from "../types/api";

export const api = {
  login: async (payload: LoginPayload) => {
    const { data } = await http.post<AuthTokenDto>("/auth/login", payload);
    return data;
  },

  getMe: async () => {
    const { data } = await http.get<Record<string, string>>("/auth/me");
    return data;
  },

  getFeatures: async () => {
    const { data } = await http.get<string[]>("/features");
    return data;
  },

  getEmployees: async (page = 1, pageSize = 20, department?: string) => {
    const { data } = await http.get<PagedResult<EmployeeSummaryDto>>("/employees", {
      params: { page, pageSize, department: department || undefined },
    });
    return data;
  },

  createEmployee: async (payload: CreateEmployeePayload) => {
    const { data } = await http.post<{ id: string }>("/employees", payload);
    return data;
  },

  updateEmployee: async (id: string, payload: UpdateEmployeePayload) => {
    await http.put(`/employees/${id}`, payload);
  },

  deleteEmployee: async (id: string) => {
    await http.delete(`/employees/${id}`);
  },

  getEmployeeById: async (id: string) => {
    const { data } = await http.get<EmployeeDto>(`/employees/${id}`);
    return data;
  },

  getLeaveByEmployee: async (employeeId: string) => {
    const { data } = await http.get<LeaveRequestDto[]>(`/leave/employee/${employeeId}`);
    return data;
  },

  getPendingLeaves: async () => {
    const { data } = await http.get<LeaveRequestDto[]>("/leave/pending");
    return data;
  },

  getLeaveBalance: async (employeeId: string, year?: number) => {
    const { data } = await http.get<LeaveBalanceDto>(`/leave/balance/${employeeId}`, {
      params: { year: year || undefined },
    });
    return data;
  },

  applyLeave: async (payload: ApplyLeavePayload) => {
    const { data } = await http.post<{ id: string }>("/leave", payload);
    return data;
  },

  approveLeave: async (leaveId: string) => {
    await http.post(`/leave/${leaveId}/approve`);
  },

  rejectLeave: async (leaveId: string, note: string) => {
    await http.post(`/leave/${leaveId}/reject`, { note });
  },

  cancelLeave: async (leaveId: string) => {
    await http.delete(`/leave/${leaveId}`);
  },

  getTenants: async () => {
    const { data } = await http.get<TenantDto[]>("/tenants");
    return data;
  },

  createTenant: async (payload: CreateTenantPayload) => {
    const { data } = await http.post<{ id: string }>("/tenants", payload);
    return data;
  },

  updateTenant: async (tenantId: string, payload: { name: string; contactEmail: string }) => {
    await http.put(`/tenants/${tenantId}`, payload);
  },

  suspendTenant: async (tenantId: string, reason: string) => {
    await http.post(`/tenants/${tenantId}/suspend`, { reason });
  },

  reinstateTenant: async (tenantId: string) => {
    await http.post(`/tenants/${tenantId}/reinstate`);
  },

  upgradePlan: async (tenantId: string, newPlan: string) => {
    await http.post(`/tenants/${tenantId}/upgrade-plan`, { newPlan });
  },

  getSubscription: async () => {
    const { data } = await http.get<SubscriptionDto>("/billing/subscription");
    return data;
  },

  createFreeSubscription: async () => {
    const { data } = await http.post<{ id: string }>("/billing/subscription/create-free");
    return data;
  },

  cancelSubscription: async (subscriptionId: string, reason: string) => {
    await http.post("/billing/subscription/cancel", { subscriptionId, reason });
  },

  getNotifications: async (page = 1, pageSize = 20, unreadOnly?: boolean) => {
    const { data } = await http.get<NotificationPagedResult>("/notifications", {
      params: { page, pageSize, unreadOnly: unreadOnly || undefined },
    });
    return data;
  },

  getUnreadCount: async () => {
    const { data } = await http.get<{ count: number }>("/notifications/unread-count");
    return data;
  },

  markRead: async (id: string) => {
    await http.put(`/notifications/${id}/read`);
  },

  markAllRead: async () => {
    await http.put("/notifications/read-all");
  },
};
