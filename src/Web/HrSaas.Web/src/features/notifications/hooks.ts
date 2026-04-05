import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { api } from "@/lib/api";
import { qk } from "@/lib/query-keys";
import { useNotify } from "@/components/feedback/useNotify";
import type {
  NotificationChannel,
  NotificationCategory,
} from "@/types/api";

export function useNotificationList(params: {
  page: number;
  pageSize: number;
  channel?: NotificationChannel;
  category?: NotificationCategory;
  unreadOnly?: boolean;
}) {
  return useQuery({
    queryKey: qk.notifications.list(params),
    queryFn: () => api.notifications.list(params),
  });
}

export function useNotificationStats() {
  return useQuery({
    queryKey: qk.notifications.stats,
    queryFn: api.notifications.getStats,
  });
}

export function useMarkRead() {
  const qc = useQueryClient();
  const notify = useNotify();

  return useMutation({
    mutationFn: api.notifications.markRead,
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
    mutationFn: api.notifications.markAllRead,
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
    mutationFn: api.notifications.retry,
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
    mutationFn: api.notifications.create,
    onSuccess: () => {
      void qc.invalidateQueries({ queryKey: ["notifications"] });
      onDone();
      notify.success("Notification sent");
    },
    onError: notify.error,
  });
}
