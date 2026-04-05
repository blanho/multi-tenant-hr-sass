import { http } from "./http";
import type {
  ApplyLeavePayload,
  AssignRolePayload,
  AuditAction,
  AuditEntityType,
  AuditLogDetailDto,
  AuditLogSummaryDto,
  AuditSeverity,
  AuthTokenDto,
  CancelSubscriptionPayload,
  CreateEmployeePayload,
  CreateNotificationPayload,
  CreateBulkNotificationPayload,
  CreateNotificationTemplatePayload,
  CreateRolePayload,
  CreateTenantPayload,
  EmployeeDto,
  EmployeeSummaryDto,
  FeatureDto,
  FileCategory,
  FileDetailDto,
  FileScanStatus,
  FileSummaryDto,
  FileUploadResult,
  FileUrlDto,
  LeaveBalanceDto,
  LeaveRequestDto,
  LoginPayload,
  NotificationCategory,
  NotificationChannel,
  NotificationDetailDto,
  NotificationPreferencesDto,
  NotificationStatsDto,
  NotificationSummaryDto,
  NotificationTemplateDto,
  PagedResult,
  RegisterPayload,
  RoleDto,
  SubscriptionDto,
  TenantDto,
  UpdateEmployeePayload,
  UpdateNotificationPreferencesPayload,
  UpdateRolePermissionsPayload,
  ActivateSubscriptionPayload,
  UpdateTenantPayload,
  UserDto,
} from "../types/api";

export const api = {
  auth: {
    register: async (payload: RegisterPayload) => {
      const { data } = await http.post<AuthTokenDto>("/auth/register", payload);
      return data;
    },
    login: async (payload: LoginPayload) => {
      const { data } = await http.post<AuthTokenDto>("/auth/login", payload);
      return data;
    },
    getMe: async () => {
      const { data } = await http.get<Record<string, string>>("/auth/me");
      return data;
    },
  },

  users: {
    list: async () => {
      const { data } = await http.get<UserDto[]>("/users");
      return data;
    },
    getById: async (id: string) => {
      const { data } = await http.get<UserDto>(`/users/${id}`);
      return data;
    },
  },

  roles: {
    list: async () => {
      const { data } = await http.get<RoleDto[]>("/roles");
      return data;
    },
    getById: async (id: string) => {
      const { data } = await http.get<RoleDto>(`/roles/${id}`);
      return data;
    },
    getAvailablePermissions: async () => {
      const { data } = await http.get<string[]>("/roles/permissions");
      return data;
    },
    create: async (payload: CreateRolePayload) => {
      const { data } = await http.post<{ id: string }>("/roles", payload);
      return data;
    },
    updatePermissions: async (id: string, payload: UpdateRolePermissionsPayload) => {
      await http.put(`/roles/${id}/permissions`, payload);
    },
    assign: async (payload: AssignRolePayload) => {
      await http.post("/roles/assign", payload);
    },
    delete: async (id: string) => {
      await http.delete(`/roles/${id}`);
    },
  },

  employees: {
    list: async (page = 1, pageSize = 20, department?: string) => {
      const { data } = await http.get<PagedResult<EmployeeSummaryDto>>("/employees", {
        params: { page, pageSize, department: department || undefined },
      });
      return data;
    },
    getById: async (id: string) => {
      const { data } = await http.get<EmployeeDto>(`/employees/${id}`);
      return data;
    },
    create: async (payload: CreateEmployeePayload) => {
      const { data } = await http.post<{ id: string }>("/employees", payload);
      return data;
    },
    update: async (id: string, payload: UpdateEmployeePayload) => {
      await http.put(`/employees/${id}`, payload);
    },
    delete: async (id: string) => {
      await http.delete(`/employees/${id}`);
    },
  },

  leave: {
    getById: async (id: string) => {
      const { data } = await http.get<LeaveRequestDto>(`/leave/${id}`);
      return data;
    },
    getByEmployee: async (employeeId: string, year?: number) => {
      const { data } = await http.get<LeaveRequestDto[]>(`/leave/employee/${employeeId}`, {
        params: { year: year || undefined },
      });
      return data;
    },
    getBalance: async (employeeId: string, year?: number) => {
      const { data } = await http.get<LeaveBalanceDto>(`/leave/balance/${employeeId}`, {
        params: { year: year || undefined },
      });
      return data;
    },
    getPending: async () => {
      const { data } = await http.get<LeaveRequestDto[]>("/leave/pending");
      return data;
    },
    apply: async (payload: ApplyLeavePayload) => {
      const { data } = await http.post<{ id: string }>("/leave", payload);
      return data;
    },
    approve: async (leaveId: string) => {
      await http.post(`/leave/${leaveId}/approve`);
    },
    reject: async (leaveId: string, note: string) => {
      await http.post(`/leave/${leaveId}/reject`, { note });
    },
    cancel: async (leaveId: string) => {
      await http.delete(`/leave/${leaveId}`);
    },
  },

  tenants: {
    list: async () => {
      const { data } = await http.get<TenantDto[]>("/tenants");
      return data;
    },
    getById: async (id: string) => {
      const { data } = await http.get<TenantDto>(`/tenants/${id}`);
      return data;
    },
    create: async (payload: CreateTenantPayload) => {
      const { data } = await http.post<{ id: string }>("/tenants", payload);
      return data;
    },
    update: async (id: string, payload: UpdateTenantPayload) => {
      await http.put(`/tenants/${id}`, payload);
    },
    suspend: async (id: string, reason: string) => {
      await http.post(`/tenants/${id}/suspend`, { reason });
    },
    reinstate: async (id: string) => {
      await http.post(`/tenants/${id}/reinstate`);
    },
    upgradePlan: async (id: string, newPlan: string) => {
      await http.post(`/tenants/${id}/upgrade-plan`, { newPlan });
    },
  },

  billing: {
    getSubscription: async () => {
      const { data } = await http.get<SubscriptionDto>("/billing/subscription");
      return data;
    },
    createFree: async () => {
      const { data } = await http.post<{ id: string }>("/billing/subscription/create-free");
      return data;
    },
    activate: async (subscriptionId: string, payload: ActivateSubscriptionPayload) => {
      await http.post(`/billing/subscription/${subscriptionId}/activate`, payload);
    },
    cancel: async (payload: CancelSubscriptionPayload) => {
      await http.post("/billing/subscription/cancel", payload);
    },
  },

  notifications: {
    list: async (params: {
      page?: number;
      pageSize?: number;
      channel?: NotificationChannel;
      category?: NotificationCategory;
      unreadOnly?: boolean;
    } = {}) => {
      const { data } = await http.get<PagedResult<NotificationSummaryDto>>("/notifications", {
        params: {
          page: params.page ?? 1,
          pageSize: params.pageSize ?? 20,
          channel: params.channel || undefined,
          category: params.category || undefined,
          unreadOnly: params.unreadOnly || undefined,
        },
      });
      return data;
    },
    getById: async (id: string) => {
      const { data } = await http.get<NotificationDetailDto>(`/notifications/${id}`);
      return data;
    },
    getStats: async () => {
      const { data } = await http.get<NotificationStatsDto>("/notifications/unread-count");
      return data;
    },
    create: async (payload: CreateNotificationPayload) => {
      const { data } = await http.post<{ id: string }>("/notifications", payload);
      return data;
    },
    markRead: async (id: string) => {
      await http.put(`/notifications/${id}/read`);
    },
    markAllRead: async () => {
      const { data } = await http.put<NotificationStatsDto>("/notifications/read-all");
      return data;
    },
    retry: async (id: string) => {
      const { data } = await http.post<NotificationDetailDto>(`/notifications/${id}/retry`);
      return data;
    },
    createBulk: async (payload: CreateBulkNotificationPayload) => {
      const { data } = await http.post<{ id: string }>("/notifications/bulk", payload);
      return data;
    },
  },

  notificationPreferences: {
    get: async () => {
      const { data } = await http.get<NotificationPreferencesDto>(
        "/notifications/preferences",
      );
      return data;
    },
    update: async (payload: UpdateNotificationPreferencesPayload) => {
      const { data } = await http.put<NotificationPreferencesDto>(
        "/notifications/preferences",
        payload,
      );
      return data;
    },
  },

  notificationTemplates: {
    list: async (channel?: NotificationChannel, isActive?: boolean) => {
      const { data } = await http.get<NotificationTemplateDto[]>(
        "/notifications/templates",
        { params: { channel: channel || undefined, isActive: isActive ?? undefined } },
      );
      return data;
    },
    create: async (payload: CreateNotificationTemplatePayload) => {
      const { data } = await http.post<{ id: string }>(
        "/notifications/templates",
        payload,
      );
      return data;
    },
  },

  features: {
    list: async () => {
      const { data } = await http.get<FeatureDto[]>("/features");
      return data;
    },
    check: async (name: string) => {
      const { data } = await http.get<FeatureDto>(`/features/${name}`);
      return data;
    },
  },

  files: {
    upload: async (
      file: File,
      category: FileCategory = "General",
      description?: string,
      entityId?: string,
      entityType?: string,
    ) => {
      const formData = new FormData();
      formData.append("file", file);
      formData.append("category", category);
      if (description) formData.append("description", description);
      if (entityId) formData.append("entityId", entityId);
      if (entityType) formData.append("entityType", entityType);

      const { data } = await http.post<FileUploadResult>("/files", formData, {
        headers: { "Content-Type": "multipart/form-data" },
      });
      return data;
    },
    getById: async (id: string) => {
      const { data } = await http.get<FileDetailDto>(`/files/${id}`);
      return data;
    },
    list: async (params: {
      page?: number;
      pageSize?: number;
      category?: FileCategory;
      scanStatus?: FileScanStatus;
      entityId?: string;
      entityType?: string;
    } = {}) => {
      const { data } = await http.get<PagedResult<FileSummaryDto>>("/files", {
        params: {
          page: params.page ?? 1,
          pageSize: params.pageSize ?? 20,
          category: params.category || undefined,
          scanStatus: params.scanStatus || undefined,
          entityId: params.entityId || undefined,
          entityType: params.entityType || undefined,
        },
      });
      return data;
    },
    getMetadata: async (id: string) => {
      const { data } = await http.get<FileDetailDto>(`/files/${id}/metadata`);
      return data;
    },
    download: (id: string) => `${http.defaults.baseURL}/files/${id}/download`,
    getUrl: async (id: string, expiresInMinutes = 60) => {
      const { data } = await http.get<FileUrlDto>(`/files/${id}/url`, {
        params: { expiresInMinutes },
      });
      return data;
    },
    delete: async (id: string) => {
      await http.delete(`/files/${id}`);
    },
  },

  auditLogs: {
    list: async (params: {
      page?: number;
      pageSize?: number;
      action?: AuditAction;
      entityType?: AuditEntityType;
      userId?: string;
      entityId?: string;
      severity?: AuditSeverity;
      from?: string;
      to?: string;
      isSystemGenerated?: boolean;
    } = {}) => {
      const { data } = await http.get<PagedResult<AuditLogSummaryDto>>("/auditlogs", {
        params: {
          page: params.page ?? 1,
          pageSize: params.pageSize ?? 50,
          action: params.action || undefined,
          entityType: params.entityType || undefined,
          userId: params.userId || undefined,
          entityId: params.entityId || undefined,
          severity: params.severity || undefined,
          from: params.from || undefined,
          to: params.to || undefined,
          isSystemGenerated: params.isSystemGenerated ?? undefined,
        },
      });
      return data;
    },
    getById: async (id: string) => {
      const { data } = await http.get<AuditLogDetailDto>(`/auditlogs/${id}`);
      return data;
    },
    getByEntity: async (
      entityType: AuditEntityType,
      entityId: string,
      page = 1,
      pageSize = 50,
    ) => {
      const { data } = await http.get<PagedResult<AuditLogSummaryDto>>(
        `/auditlogs/entity/${entityType}/${entityId}`,
        { params: { page, pageSize } },
      );
      return data;
    },
  },
};
