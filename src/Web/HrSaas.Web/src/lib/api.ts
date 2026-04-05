import { authApi } from "@/features/auth/api";
import { usersApi } from "@/features/users/api";
import { rolesApi } from "@/features/roles/api";
import { employeesApi } from "@/features/employees/api";
import { leaveApi } from "@/features/leave/api";
import { tenantsApi } from "@/features/tenants/api";
import { billingApi } from "@/features/billing/api";
import {
  notificationsApi,
  notificationPreferencesApi,
  notificationTemplatesApi,
} from "@/features/notifications/api";
import { featuresApi } from "@/features/dashboard/api";
import { filesApi } from "@/features/files/api";
import { auditLogsApi } from "@/features/audit/api";

export const api = {
  auth: authApi,
  users: usersApi,
  roles: rolesApi,
  employees: employeesApi,
  leave: leaveApi,
  tenants: tenantsApi,
  billing: billingApi,
  notifications: notificationsApi,
  notificationPreferences: notificationPreferencesApi,
  notificationTemplates: notificationTemplatesApi,
  features: featuresApi,
  files: filesApi,
  auditLogs: auditLogsApi,
};
