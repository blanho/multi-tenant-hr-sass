import { Box, Drawer, Stack, Typography } from "@mui/material";
import { StatusChip } from "@/components";
import dayjs from "dayjs";
import type { NotificationDetailDto } from "./types";

interface NotificationDetailDrawerProps {
  open: boolean;
  onClose: () => void;
  notification: NotificationDetailDto | null;
}

export function NotificationDetailDrawer({
  open,
  onClose,
  notification,
}: Readonly<NotificationDetailDrawerProps>) {
  return (
    <Drawer
      anchor="right"
      open={open}
      onClose={onClose}
      slotProps={{ paper: { sx: { width: 400, p: 3 } } }}
    >
      {notification && (
        <Stack spacing={2}>
          <Typography variant="h6">Notification Detail</Typography>
          <StatusChip status={notification.status} />
          <Typography variant="body2" color="text.secondary">
            {notification.channel} &middot; {notification.category} &middot;{" "}
            {notification.priority}
          </Typography>
          {notification.subject && (
            <Typography variant="body2">
              <strong>Subject:</strong> {notification.subject}
            </Typography>
          )}
          {notification.body && (
            <Box
              sx={{
                p: 1.5,
                borderRadius: 1,
                bgcolor: "grey.50",
                whiteSpace: "pre-wrap",
                fontSize: 13,
              }}
            >
              {notification.body}
            </Box>
          )}
          {notification.deliveredAt && (
            <Typography variant="caption" color="text.disabled">
              Delivered {dayjs(notification.deliveredAt).format("MMM D, YYYY h:mm A")}
            </Typography>
          )}
          {notification.readAt && (
            <Typography variant="caption" color="text.disabled">
              Read {dayjs(notification.readAt).format("MMM D, YYYY h:mm A")}
            </Typography>
          )}
          {notification.retryCount > 0 && (
            <Typography variant="caption" color="warning.main">
              Retried {notification.retryCount} / {notification.maxRetries} times
            </Typography>
          )}
          {notification.metadata && (
            <Box sx={{ p: 1.5, borderRadius: 1, bgcolor: "grey.100" }}>
              <Typography
                variant="caption"
                component="pre"
                sx={{ whiteSpace: "pre-wrap" }}
              >
                {notification.metadata}
              </Typography>
            </Box>
          )}
        </Stack>
      )}
    </Drawer>
  );
}
