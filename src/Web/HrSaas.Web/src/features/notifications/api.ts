import { http } from "@/lib/http";
import type { PagedResult, NotificationChannel, NotificationCategory } from "@/types/shared";
import type {
  NotificationSummaryDto,
  NotificationDetailDto,
  NotificationStatsDto,
  NotificationPreferencesDto,
  NotificationTemplateDto,
  CreateNotificationPayload,
  CreateBulkNotificationPayload,
  UpdateNotificationPreferencesPayload,
  CreateNotificationTemplatePayload,
} from "./types";

export const notificationsApi = {
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
};

export const notificationPreferencesApi = {
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
};

export const notificationTemplatesApi = {
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
};
