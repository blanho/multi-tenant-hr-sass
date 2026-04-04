import HistoryRoundedIcon from "@mui/icons-material/HistoryRounded";
import InfoRoundedIcon from "@mui/icons-material/InfoRounded";
import {
  Box,
  Card,
  CardContent,
  Chip,
  Collapse,
  Drawer,
  FormControl,
  IconButton,
  InputLabel,
  MenuItem,
  Select,
  Stack,
  TextField,
  Tooltip,
  Typography,
} from "@mui/material";
import type { GridColDef, GridPaginationModel } from "@mui/x-data-grid";
import { DataGrid } from "@mui/x-data-grid";
import { useQuery } from "@tanstack/react-query";
import dayjs from "dayjs";
import { useCallback, useMemo, useState } from "react";
import { EmptyState } from "../../components/common/EmptyState";
import { PageHeader } from "../../components/common/PageHeader";
import { StatusChip } from "../../components/common/StatusChip";
import { useNotify } from "../../components/feedback/useNotify";
import { api } from "../../lib/api";
import { qk } from "../../lib/query-keys";
import type {
  AuditAction,
  AuditEntityType,
  AuditLogDetailDto,
  AuditLogSummaryDto,
  AuditSeverity,
} from "../../types/api";

const ACTIONS: AuditAction[] = [
  "Created", "Updated", "Deleted", "Viewed", "Login", "Logout",
  "PasswordChanged", "RoleAssigned", "RoleRemoved", "PermissionGranted",
  "PermissionRevoked", "TenantCreated", "TenantSuspended", "TenantReinstated",
  "PlanUpgraded", "SubscriptionCreated", "SubscriptionCancelled",
  "LeaveApplied", "LeaveApproved", "LeaveRejected", "LeaveCancelled",
];

const ENTITY_TYPES: AuditEntityType[] = [
  "User", "Employee", "Leave", "Tenant", "Role", "Subscription", "Notification", "File",
];

const SEVERITIES: AuditSeverity[] = ["Info", "Warning", "Error"];

function AuditEmpty() {
  return (
    <EmptyState
      title="No Audit Logs"
      description="No audit trail entries found for the current filters"
      icon={<HistoryRoundedIcon sx={{ fontSize: 48 }} />}
    />
  );
}

export function AuditLogsPage() {
  const notify = useNotify();

  const [paginationModel, setPaginationModel] = useState<GridPaginationModel>({
    page: 0,
    pageSize: 50,
  });
  const [action, setAction] = useState<AuditAction | "">("");
  const [entityType, setEntityType] = useState<AuditEntityType | "">("");
  const [severity, setSeverity] = useState<AuditSeverity | "">("");
  const [from, setFrom] = useState("");
  const [to, setTo] = useState("");
  const filtersOpen = true;
  const [detailOpen, setDetailOpen] = useState(false);
  const [selected, setSelected] = useState<AuditLogDetailDto | null>(null);

  const filterParams = useMemo(
    () => ({
      page: paginationModel.page + 1,
      pageSize: paginationModel.pageSize,
      action: action || undefined,
      entityType: entityType || undefined,
      severity: severity || undefined,
      from: from || undefined,
      to: to || undefined,
    }),
    [paginationModel.page, paginationModel.pageSize, action, entityType, severity, from, to],
  );

  const listQuery = useQuery({
    queryKey: qk.auditLogs.list(filterParams),
    queryFn: () => api.auditLogs.list(filterParams),
  });

  const openDetail = useCallback(
    async (id: string) => {
      try {
        const detail = await api.auditLogs.getById(id);
        setSelected(detail);
        setDetailOpen(true);
      } catch {
        notify.error("Failed to load audit log details");
      }
    },
    [notify],
  );

  const columns = useMemo<GridColDef<AuditLogSummaryDto>[]>(
    () => [
      {
        field: "severity",
        headerName: "Severity",
        width: 100,
        renderCell: ({ value }) => <StatusChip status={value} />,
      },
      {
        field: "action",
        headerName: "Action",
        width: 160,
        renderCell: ({ value }) => <Chip label={value} size="small" variant="outlined" />,
      },
      {
        field: "entityType",
        headerName: "Entity Type",
        width: 120,
      },
      {
        field: "description",
        headerName: "Description",
        flex: 1,
        minWidth: 250,
      },
      {
        field: "userEmail",
        headerName: "User",
        width: 180,
        renderCell: ({ value }) => (
          <Typography variant="body2" color={value ? "text.primary" : "text.disabled"}>
            {value ?? "System"}
          </Typography>
        ),
      },
      {
        field: "createdAt",
        headerName: "Timestamp",
        width: 170,
        valueFormatter: (value: string) => dayjs(value).format("MMM D, YYYY h:mm A"),
      },
      {
        field: "detail",
        headerName: "",
        width: 50,
        sortable: false,
        filterable: false,
        renderCell: ({ row }) => (
          <Tooltip title="View Details">
            <IconButton size="small" onClick={() => void openDetail(row.id)}>
              <InfoRoundedIcon fontSize="small" />
            </IconButton>
          </Tooltip>
        ),
      },
    ],
    [openDetail],
  );

  const rows = listQuery.data?.items ?? [];
  const rowCount = listQuery.data?.totalCount ?? 0;

  return (
    <Stack spacing={2.5}>
      <PageHeader
        title="Audit Logs"
        subtitle="View tenant activity, security events, and system changes"
      />

      <Card>
        <CardContent>
          <Collapse in={filtersOpen}>
            <Stack direction="row" spacing={2} mb={2} flexWrap="wrap" useFlexGap>
              <FormControl size="small" sx={{ minWidth: 160 }}>
                <InputLabel>Action</InputLabel>
                <Select
                  value={action}
                  label="Action"
                  onChange={(e) => {
                    setAction(e.target.value as AuditAction | "");
                    setPaginationModel((m) => ({ ...m, page: 0 }));
                  }}
                >
                  <MenuItem value="">All</MenuItem>
                  {ACTIONS.map((a) => (
                    <MenuItem key={a} value={a}>{a}</MenuItem>
                  ))}
                </Select>
              </FormControl>

              <FormControl size="small" sx={{ minWidth: 140 }}>
                <InputLabel>Entity Type</InputLabel>
                <Select
                  value={entityType}
                  label="Entity Type"
                  onChange={(e) => {
                    setEntityType(e.target.value as AuditEntityType | "");
                    setPaginationModel((m) => ({ ...m, page: 0 }));
                  }}
                >
                  <MenuItem value="">All</MenuItem>
                  {ENTITY_TYPES.map((t) => (
                    <MenuItem key={t} value={t}>{t}</MenuItem>
                  ))}
                </Select>
              </FormControl>

              <FormControl size="small" sx={{ minWidth: 120 }}>
                <InputLabel>Severity</InputLabel>
                <Select
                  value={severity}
                  label="Severity"
                  onChange={(e) => {
                    setSeverity(e.target.value as AuditSeverity | "");
                    setPaginationModel((m) => ({ ...m, page: 0 }));
                  }}
                >
                  <MenuItem value="">All</MenuItem>
                  {SEVERITIES.map((s) => (
                    <MenuItem key={s} value={s}>{s}</MenuItem>
                  ))}
                </Select>
              </FormControl>

              <TextField
                size="small"
                label="From"
                type="date"
                slotProps={{ inputLabel: { shrink: true } }}
                value={from}
                onChange={(e) => {
                  setFrom(e.target.value);
                  setPaginationModel((m) => ({ ...m, page: 0 }));
                }}
                sx={{ width: 160 }}
              />

              <TextField
                size="small"
                label="To"
                type="date"
                slotProps={{ inputLabel: { shrink: true } }}
                value={to}
                onChange={(e) => {
                  setTo(e.target.value);
                  setPaginationModel((m) => ({ ...m, page: 0 }));
                }}
                sx={{ width: 160 }}
              />
            </Stack>
          </Collapse>

          <Box sx={{ height: 540 }}>
            <DataGrid
              rows={rows}
              columns={columns}
              rowCount={rowCount}
              loading={listQuery.isFetching}
              paginationMode="server"
              paginationModel={paginationModel}
              onPaginationModelChange={setPaginationModel}
              pageSizeOptions={[25, 50, 100]}
              disableRowSelectionOnClick
              slots={{ noRowsOverlay: AuditEmpty }}
              slotProps={{ noRowsOverlay: {} }}
              density="compact"
              sx={{ border: 0 }}
            />
          </Box>
        </CardContent>
      </Card>

      <Drawer
        anchor="right"
        open={detailOpen}
        onClose={() => setDetailOpen(false)}
        slotProps={{ paper: { sx: { width: 440, p: 3 } } }}
      >
        {selected && (
          <Stack spacing={2}>
            <Typography variant="h6">Audit Log Detail</Typography>
            <StatusChip status={selected.severity} />
            <Stack spacing={0.5}>
              <DetailRow label="Action" value={selected.action} />
              <DetailRow label="Entity" value={`${selected.entityType} / ${selected.entityId}`} />
              <DetailRow label="Description" value={selected.description} />
              <DetailRow label="User" value={selected.userEmail ?? "System"} />
              <DetailRow label="IP Address" value={selected.ipAddress ?? "—"} />
              <DetailRow label="Duration" value={`${selected.durationMs}ms`} />
              <DetailRow
                label="Timestamp"
                value={dayjs(selected.createdAt).format("MMM D, YYYY h:mm:ss A")}
              />
              {selected.requestMethod && selected.requestPath && (
                <DetailRow
                  label="Request"
                  value={`${selected.requestMethod} ${selected.requestPath}`}
                />
              )}
              {selected.correlationId && (
                <DetailRow label="Correlation ID" value={selected.correlationId} />
              )}
            </Stack>

            {selected.oldValues && (
              <JsonBlock title="Old Values" json={selected.oldValues} />
            )}
            {selected.newValues && (
              <JsonBlock title="New Values" json={selected.newValues} />
            )}
            {selected.additionalData && (
              <JsonBlock title="Additional Data" json={selected.additionalData} />
            )}
          </Stack>
        )}
      </Drawer>
    </Stack>
  );
}

function DetailRow({ label, value }: Readonly<{ label: string; value: string }>) {
  return (
    <Stack direction="row" justifyContent="space-between" py={0.5}>
      <Typography variant="body2" color="text.secondary" sx={{ minWidth: 100 }}>
        {label}
      </Typography>
      <Typography
        variant="body2"
        fontWeight={500}
        sx={{ maxWidth: 280, wordBreak: "break-all", textAlign: "right" }}
      >
        {value}
      </Typography>
    </Stack>
  );
}

function JsonBlock({ title, json }: Readonly<{ title: string; json: string }>) {
  return (
    <Stack spacing={0.5}>
      <Typography variant="subtitle2">{title}</Typography>
      <Box
        sx={{
          p: 1.5,
          borderRadius: 1,
          bgcolor: "grey.50",
          fontFamily: "monospace",
          fontSize: 12,
          whiteSpace: "pre-wrap",
          wordBreak: "break-all",
          maxHeight: 200,
          overflow: "auto",
        }}
      >
        {json}
      </Box>
    </Stack>
  );
}
