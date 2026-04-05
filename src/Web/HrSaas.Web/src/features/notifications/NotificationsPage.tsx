import AddRoundedIcon from "@mui/icons-material/AddRounded";
import CheckCircleRoundedIcon from "@mui/icons-material/CheckCircleRounded";
import DoneAllRoundedIcon from "@mui/icons-material/DoneAllRounded";
import FilterListRoundedIcon from "@mui/icons-material/FilterListRounded";
import MarkEmailReadRoundedIcon from "@mui/icons-material/MarkEmailReadRounded";
import NotificationsNoneRoundedIcon from "@mui/icons-material/NotificationsNoneRounded";
import ReplayRoundedIcon from "@mui/icons-material/ReplayRounded";
import {
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  Collapse,
  FormControl,
  FormControlLabel,
  Grid,
  IconButton,
  InputLabel,
  MenuItem,
  Select,
  Stack,
  Switch,
  Tooltip,
  Typography,
} from "@mui/material";
import type { GridColDef, GridPaginationModel } from "@mui/x-data-grid";
import { DataGrid } from "@mui/x-data-grid";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import dayjs from "dayjs";
import relativeTime from "dayjs/plugin/relativeTime";
import { useCallback, useMemo, useState } from "react";
import { EmptyState, PageHeader, StatCard, StatusChip } from "@/components";
import { useNotify } from "@/hooks/useNotify";
import { api } from "@/lib/api";
import type { NotificationCategory, NotificationChannel } from "@/types/shared";
import type { NotificationDetailDto, NotificationSummaryDto } from "./types";
import { CATEGORIES, CHANNELS } from "./constants";
import { useNotificationList, useNotificationStats, useSendNotification } from "./hooks";
import { NotificationDetailDrawer } from "./NotificationDetailDrawer";
import { SendNotificationDialog } from "./SendNotificationDialog";

dayjs.extend(relativeTime);

function NotificationsEmpty() {
  return (
    <EmptyState
      title="No Notifications"
      description="You are all caught up!"
      icon={<NotificationsNoneRoundedIcon sx={{ fontSize: 48 }} />}
    />
  );
}

export function NotificationsPage() {
  const notify = useNotify();
  const queryClient = useQueryClient();

  const [paginationModel, setPaginationModel] = useState<GridPaginationModel>({
    page: 0,
    pageSize: 20,
  });
  const [channel, setChannel] = useState<NotificationChannel | "">("");
  const [category, setCategory] = useState<NotificationCategory | "">("");
  const [unreadOnly, setUnreadOnly] = useState(false);
  const [filtersOpen, setFiltersOpen] = useState(false);
  const [detailOpen, setDetailOpen] = useState(false);
  const [selected, setSelected] = useState<NotificationDetailDto | null>(null);
  const [sendOpen, setSendOpen] = useState(false);

  const sendMutation = useSendNotification(() => setSendOpen(false));

  const filterParams = useMemo(
    () => ({
      page: paginationModel.page + 1,
      pageSize: paginationModel.pageSize,
      channel: channel || undefined,
      category: category || undefined,
      unreadOnly: unreadOnly || undefined,
    }),
    [paginationModel.page, paginationModel.pageSize, channel, category, unreadOnly],
  );

  const listQuery = useNotificationList(filterParams);
  const statsQuery = useNotificationStats();

  const markReadMutation = useMutation({
    mutationFn: api.notifications.markRead,
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ["notifications"] });
      notify.success("Marked as read");
    },
    onError: notify.error,
  });

  const markAllReadMutation = useMutation({
    mutationFn: api.notifications.markAllRead,
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ["notifications"] });
      notify.success("All notifications marked as read");
    },
    onError: notify.error,
  });

  const retryMutation = useMutation({
    mutationFn: api.notifications.retry,
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ["notifications"] });
      notify.success("Notification queued for retry");
    },
    onError: notify.error,
  });

  const openDetail = useCallback(
    async (id: string) => {
      try {
        const detail = await api.notifications.getById(id);
        setSelected(detail);
        setDetailOpen(true);
      } catch {
        notify.error("Failed to load notification details");
      }
    },
    [notify],
  );

  const columns = useMemo<GridColDef<NotificationSummaryDto>[]>(
    () => [
      {
        field: "status",
        headerName: "Status",
        width: 110,
        renderCell: ({ value }) => <StatusChip status={value} />,
      },
      {
        field: "channel",
        headerName: "Channel",
        width: 100,
        renderCell: ({ value }) => <Chip label={value} size="small" variant="outlined" />,
      },
      {
        field: "category",
        headerName: "Category",
        width: 130,
        renderCell: ({ value }) => (
          <Typography variant="body2" noWrap>
            {value}
          </Typography>
        ),
      },
      { field: "subject", headerName: "Subject", flex: 1, minWidth: 200 },
      {
        field: "priority",
        headerName: "Priority",
        width: 100,
        renderCell: ({ value }) => <Chip label={value} size="small" />,
      },
      {
        field: "createdAt",
        headerName: "Sent At",
        width: 150,
        valueFormatter: (value: string) => dayjs(value).fromNow(),
      },
      {
        field: "actions",
        headerName: "",
        width: 130,
        sortable: false,
        filterable: false,
        renderCell: ({ row }) => (
          <Stack direction="row" spacing={0.5}>
            <Tooltip title="View Details">
              <IconButton size="small" onClick={() => void openDetail(row.id)}>
                <MarkEmailReadRoundedIcon fontSize="small" />
              </IconButton>
            </Tooltip>
            {(row.status === "Delivered" || row.status === "Sent") && (
              <Tooltip title="Mark Read">
                <IconButton size="small" onClick={() => markReadMutation.mutate(row.id)}>
                  <CheckCircleRoundedIcon fontSize="small" />
                </IconButton>
              </Tooltip>
            )}
            {row.status === "Failed" && (
              <Tooltip title="Retry">
                <IconButton
                  size="small"
                  color="warning"
                  onClick={() => retryMutation.mutate(row.id)}
                >
                  <ReplayRoundedIcon fontSize="small" />
                </IconButton>
              </Tooltip>
            )}
          </Stack>
        ),
      },
    ],
    [openDetail, markReadMutation, retryMutation],
  );

  const stats = statsQuery.data;
  const rows = listQuery.data?.items ?? [];
  const rowCount = listQuery.data?.totalCount ?? 0;

  return (
    <Stack spacing={2.5}>
      <PageHeader
        title="Notifications"
        subtitle="View, filter, and manage notification delivery"
        actions={
          <Stack direction="row" spacing={1}>
            <Button
              variant="outlined"
              startIcon={<AddRoundedIcon />}
              onClick={() => setSendOpen(true)}
            >
              Send Notification
            </Button>
            <Button
              variant="outlined"
              startIcon={<DoneAllRoundedIcon />}
              onClick={() => markAllReadMutation.mutate()}
              disabled={markAllReadMutation.isPending}
            >
              Mark All Read
            </Button>
          </Stack>
        }
      />

      {stats && (
        <Grid container spacing={2}>
          <Grid size={{ xs: 6, md: 3 }}>
            <StatCard label="Total" value={stats.totalCount} />
          </Grid>
          <Grid size={{ xs: 6, md: 3 }}>
            <StatCard label="Delivered" value={stats.deliveredCount} />
          </Grid>
          <Grid size={{ xs: 6, md: 3 }}>
            <StatCard label="Failed" value={stats.failedCount} />
          </Grid>
          <Grid size={{ xs: 6, md: 3 }}>
            <StatCard label="Unread" value={stats.unreadCount} />
          </Grid>
        </Grid>
      )}

      <Card>
        <CardContent>
          <Stack direction="row" justifyContent="space-between" alignItems="center" mb={1}>
            <Typography variant="subtitle2" color="text.secondary">
              {rowCount} notification{rowCount === 1 ? "" : "s"}
            </Typography>
            <Button
              size="small"
              startIcon={<FilterListRoundedIcon />}
              onClick={() => setFiltersOpen((o) => !o)}
            >
              Filters
            </Button>
          </Stack>

          <Collapse in={filtersOpen}>
            <Stack direction="row" spacing={2} mb={2} alignItems="center">
              <FormControl size="small" sx={{ minWidth: 130 }}>
                <InputLabel>Channel</InputLabel>
                <Select
                  value={channel}
                  label="Channel"
                  onChange={(e) => {
                    setChannel(e.target.value as NotificationChannel | "");
                    setPaginationModel((m) => ({ ...m, page: 0 }));
                  }}
                >
                  <MenuItem value="">All</MenuItem>
                  {CHANNELS.map((c) => (
                    <MenuItem key={c} value={c}>
                      {c}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>

              <FormControl size="small" sx={{ minWidth: 150 }}>
                <InputLabel>Category</InputLabel>
                <Select
                  value={category}
                  label="Category"
                  onChange={(e) => {
                    setCategory(e.target.value as NotificationCategory | "");
                    setPaginationModel((m) => ({ ...m, page: 0 }));
                  }}
                >
                  <MenuItem value="">All</MenuItem>
                  {CATEGORIES.map((c) => (
                    <MenuItem key={c} value={c}>
                      {c}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>

              <FormControlLabel
                control={
                  <Switch
                    size="small"
                    checked={unreadOnly}
                    onChange={(_, v) => {
                      setUnreadOnly(v);
                      setPaginationModel((m) => ({ ...m, page: 0 }));
                    }}
                  />
                }
                label="Unread Only"
              />
            </Stack>
          </Collapse>

          <Box sx={{ height: 520 }}>
            <DataGrid
              rows={rows}
              columns={columns}
              rowCount={rowCount}
              loading={listQuery.isFetching}
              paginationMode="server"
              paginationModel={paginationModel}
              onPaginationModelChange={setPaginationModel}
              pageSizeOptions={[10, 20, 50]}
              disableRowSelectionOnClick
              slots={{ noRowsOverlay: NotificationsEmpty }}
              slotProps={{ noRowsOverlay: {} }}
              density="compact"
              sx={{ border: 0 }}
            />
          </Box>
        </CardContent>
      </Card>

      <NotificationDetailDrawer
        open={detailOpen}
        onClose={() => setDetailOpen(false)}
        notification={selected}
      />

      <SendNotificationDialog
        open={sendOpen}
        loading={sendMutation.isPending}
        onClose={() => setSendOpen(false)}
        onSubmit={(payload) => sendMutation.mutate(payload)}
      />
    </Stack>
  );
}
