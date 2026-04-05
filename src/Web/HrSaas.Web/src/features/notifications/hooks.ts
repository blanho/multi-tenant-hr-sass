import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { qk } from "@/lib/query-keys";
import { useNotify } from "@/hooks/useNotify";
import type { NotificationChannel, NotificationCategory } from "@/types/shared";
import { notificationsApi } from "./api";

export function useNotificationList(params: {
  page: number;
  pageSize: number;
  channel?: NotificationChannel;
  category?: NotificationCategory;
  unreadOnly?: boolean;
}) {
  return useQuery({
    queryKey: qk.notifications.list(params),
    queryFn: () => notificationsApi.list(params),
  });
}

export function useNotificationStats() {
  return useQuery({
    queryKey: qk.notifications.stats,
    queryFn: notificationsApi.getStats,
  });
}

export function useMarkRead() {
  const qc = useQueryClient();
  const notify = useNotify();

  return useMutation({
    mutationFn: notificationsApi.markRead,
    onSuccess: () => {
      void qc.invalidateQueries({ queryKey: ["notifications"] });
      notify.success("Marked as read");
    },
    onError: notify.error,
  });
}

export function useMarkAllRead() {
  const qc = useQueryClient();
  const notify = useNotify();

  return useMutation({
    mutationFn: notificationsApi.markAllRead,
    onSuccess: () => {
      void qc.invalidateQueries({ queryKey: ["notifications"] });
      notify.success("All notifications marked as read");
    },
    onError: notify.error,
  });
}

export function useRetryNotification() {
  const qc = useQueryClient();
  const notify = useNotify();

  return useMutation({
    mutationFn: notificationsApi.retry,
    onSuccess: () => {
      void qc.invalidateQueries({ queryKey: ["notifications"] });
      notify.success("Notification queued for retry");
    },
    onError: notify.error,
  });
}

export function useSendNotification(onDone: () => void) {
  const qc = useQueryClient();
  const notify = useNotify();

  return useMutation({
    mutationFn: notificationsApi.create,
    onSuccess: () => {
      void qc.invalidateQueries({ queryKey: ["notifications"] });
      onDone();
      notify.success("Notification sent");
    },
    onError: notify.error,
  });
}
