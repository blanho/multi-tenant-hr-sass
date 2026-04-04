import {
  Alert,
  Button,
  Card,
  CardContent,
  Chip,
  Stack,
  Typography,
} from "@mui/material";
import { DataGrid, type GridColDef } from "@mui/x-data-grid";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useMemo } from "react";
import { PageHeader } from "../../components/common/PageHeader";
import { api } from "../../lib/api";
import { extractErrorMessage } from "../../lib/http";
import type { NotificationDto } from "../../types/api";

export function NotificationsPage() {
  const queryClient = useQueryClient();

  const notificationsQuery = useQuery({
    queryKey: ["notifications", 1, 50],
    queryFn: () => api.getNotifications(1, 50),
  });

  const markReadMutation = useMutation({
    mutationFn: api.markRead,
    onSuccess: () => void queryClient.invalidateQueries({ queryKey: ["notifications"] }),
  });

  const markAllMutation = useMutation({
    mutationFn: api.markAllRead,
    onSuccess: () => void queryClient.invalidateQueries({ queryKey: ["notifications"] }),
  });

  const columns = useMemo<GridColDef<NotificationDto>[]>(
    () => [
      { field: "subject", headerName: "Subject", flex: 1.4, minWidth: 220 },
      { field: "channel", headerName: "Channel", width: 120 },
      { field: "category", headerName: "Category", width: 140 },
      {
        field: "priority",
        headerName: "Priority",
        width: 120,
        renderCell: ({ value }) => <Chip size="small" label={String(value)} />,
      },
      {
        field: "status",
        headerName: "Status",
        width: 120,
      },
      {
        field: "actions",
        headerName: "Actions",
        width: 140,
        sortable: false,
        renderCell: ({ row }) => (
          <Button size="small" onClick={() => markReadMutation.mutate(row.id)}>
            Mark Read
          </Button>
        ),
      },
    ],
    [markReadMutation],
  );

  return (
    <Stack spacing={2.5}>
      <PageHeader
        title="Notifications"
        subtitle="Unified inbox across channels with status visibility and bulk actions"
        actions={
          <Button variant="outlined" onClick={() => markAllMutation.mutate()}>
            Mark All Read
          </Button>
        }
      />

      {(notificationsQuery.isError || markReadMutation.isError || markAllMutation.isError) && (
        <Alert severity="error">
          {extractErrorMessage(
            notificationsQuery.error ?? markReadMutation.error ?? markAllMutation.error,
          )}
        </Alert>
      )}

      <Card>
        <CardContent>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
            Total notifications: {notificationsQuery.data?.totalCount ?? 0}
          </Typography>
          <DataGrid
            autoHeight
            rows={notificationsQuery.data?.items ?? []}
            columns={columns}
            loading={notificationsQuery.isLoading}
            disableRowSelectionOnClick
            pageSizeOptions={[10, 20, 50]}
          />
        </CardContent>
      </Card>
    </Stack>
  );
}
