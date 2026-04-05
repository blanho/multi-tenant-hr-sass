import CheckCircleRoundedIcon from "@mui/icons-material/CheckCircleRounded";
import CancelRoundedIcon from "@mui/icons-material/CancelRounded";
import {
  Autocomplete,
  Button,
  Card,
  CardContent,
  Chip,
  Grid,
  LinearProgress,
  Stack,
  TextField,
  Typography,
} from "@mui/material";
import { DataGrid, type GridColDef } from "@mui/x-data-grid";
import dayjs from "dayjs";
import { useMemo, useState } from "react";
import { EmptyState, PageHeader, StatusChip } from "@/components";
import { api } from "@/lib/api";
import { qk } from "@/lib/query-keys";
import { useQuery } from "@tanstack/react-query";
import { useAuth } from "../auth/auth-context";
import type { LeaveRequestDto } from "./types";
import {
  useApplyLeave,
  useApproveLeave,
  useCancelLeave,
  useLeaveBalance,
  useLeaveHistory,
  usePendingLeave,
  useRejectLeave,
} from "./hooks";
import { ApplyLeaveDialog } from "./ApplyLeaveDialog";
import { RejectLeaveDialog } from "./RejectLeaveDialog";
import type { ApplyLeaveForm } from "./schemas";

function PendingEmpty() {
  return (
    <EmptyState
      title="No pending requests"
      description="All leave requests have been processed"
    />
  );
}

function HistoryEmpty() {
  return (
    <EmptyState
      title="No leave records"
      description="This employee has no leave requests for the current year"
    />
  );
}

export function LeavePage() {
  const { tenantId } = useAuth();
  const [selectedEmployeeId, setSelectedEmployeeId] = useState<string | null>(null);
  const [applyOpen, setApplyOpen] = useState(false);
  const [rejectOpen, setRejectOpen] = useState<string | null>(null);

  const employeesQuery = useQuery({
    queryKey: qk.employees.list(1, 200),
    queryFn: () => api.employees.list(1, 200),
  });

  const pendingQuery = usePendingLeave();
  const balanceQuery = useLeaveBalance(selectedEmployeeId, dayjs().year());
  const historyQuery = useLeaveHistory(selectedEmployeeId, dayjs().year());

  const applyMutation = useApplyLeave(() => setApplyOpen(false));
  const approveMutation = useApproveLeave();
  const rejectMutation = useRejectLeave(() => setRejectOpen(null));
  const cancelMutation = useCancelLeave();

  const employees = useMemo(
    () => employeesQuery.data?.items ?? [],
    [employeesQuery.data?.items],
  );

  const pendingColumns = useMemo<GridColDef<LeaveRequestDto>[]>(
    () => [
      {
        field: "employeeId",
        headerName: "Employee",
        minWidth: 180,
        flex: 1,
        valueGetter: (_value: string, row: LeaveRequestDto) => {
          const emp = employees.find((e) => e.id === row.employeeId);
          return emp?.name ?? row.employeeId.slice(0, 8);
        },
      },
      { field: "type", headerName: "Type", width: 120 },
      {
        field: "startDate",
        headerName: "From",
        width: 120,
        valueFormatter: (value: string) => dayjs(value).format("MMM D, YYYY"),
      },
      {
        field: "endDate",
        headerName: "To",
        width: 120,
        valueFormatter: (value: string) => dayjs(value).format("MMM D, YYYY"),
      },
      { field: "durationDays", headerName: "Days", width: 80, align: "center" },
      {
        field: "status",
        headerName: "Status",
        width: 120,
        renderCell: ({ value }) => <StatusChip status={String(value)} />,
      },
      {
        field: "actions",
        headerName: "Actions",
        width: 200,
        sortable: false,
        renderCell: ({ row }) => (
          <Stack direction="row" spacing={0.5} alignItems="center">
            <Button
              size="small"
              color="success"
              startIcon={<CheckCircleRoundedIcon />}
              onClick={() => approveMutation.mutate(row.id)}
              disabled={approveMutation.isPending}
            >
              Approve
            </Button>
            <Button
              size="small"
              color="error"
              startIcon={<CancelRoundedIcon />}
              onClick={() => setRejectOpen(row.id)}
            >
              Reject
            </Button>
          </Stack>
        ),
      },
    ],
    [approveMutation, employees],
  );

  const historyColumns = useMemo<GridColDef<LeaveRequestDto>[]>(
    () => [
      { field: "type", headerName: "Type", width: 120 },
      {
        field: "startDate",
        headerName: "From",
        width: 120,
        valueFormatter: (value: string) => dayjs(value).format("MMM D, YYYY"),
      },
      {
        field: "endDate",
        headerName: "To",
        width: 120,
        valueFormatter: (value: string) => dayjs(value).format("MMM D, YYYY"),
      },
      { field: "durationDays", headerName: "Days", width: 80 },
      {
        field: "status",
        headerName: "Status",
        width: 120,
        renderCell: ({ value }) => <StatusChip status={String(value)} />,
      },
      { field: "reason", headerName: "Reason", flex: 1, minWidth: 180 },
      {
        field: "actions",
        headerName: "",
        width: 100,
        sortable: false,
        renderCell: ({ row }) =>
          row.status === "Pending" ? (
            <Button size="small" color="error" onClick={() => cancelMutation.mutate(row.id)}>
              Cancel
            </Button>
          ) : null,
      },
    ],
    [cancelMutation],
  );

  const balance = balanceQuery.data;

  return (
    <Stack spacing={2.5}>
      <PageHeader
        title="Leave Management"
        subtitle="Apply, approve, reject, and monitor leave balances"
        actions={
          <Button variant="contained" onClick={() => setApplyOpen(true)}>
            Apply for Leave
          </Button>
        }
      />

      {(pendingQuery.isFetching || employeesQuery.isFetching) && <LinearProgress />}

      <Grid container spacing={2}>
        <Grid size={{ xs: 12, md: 4 }}>
          <Card>
            <CardContent>
              <Typography variant="h6" mb={2}>
                Employee Lookup
              </Typography>
              <Autocomplete
                options={employees}
                getOptionLabel={(o) => `${o.name} (${o.department})`}
                value={employees.find((e) => e.id === selectedEmployeeId) ?? null}
                onChange={(_, v) => setSelectedEmployeeId(v?.id ?? null)}
                renderInput={(params) => (
                  <TextField {...params} label="Select employee" size="small" />
                )}
              />

              {balance && (
                <Stack spacing={1} mt={2}>
                  <Stack direction="row" justifyContent="space-between">
                    <Typography variant="body2" color="text.secondary">
                      Annual
                    </Typography>
                    <Chip
                      size="small"
                      label={`${balance.annualRemaining} / ${balance.annualAllowance}`}
                      color={balance.annualRemaining > 0 ? "success" : "error"}
                    />
                  </Stack>
                  <Stack direction="row" justifyContent="space-between">
                    <Typography variant="body2" color="text.secondary">
                      Sick
                    </Typography>
                    <Chip
                      size="small"
                      label={`${balance.sickRemaining} / ${balance.sickAllowance}`}
                      color={balance.sickRemaining > 0 ? "success" : "error"}
                    />
                  </Stack>
                  <Typography variant="caption" color="text.disabled">
                    Year {balance.year}
                  </Typography>
                </Stack>
              )}
            </CardContent>
          </Card>
        </Grid>

        <Grid size={{ xs: 12, md: 8 }}>
          <Card>
            <CardContent>
              <Typography variant="h6" mb={2}>
                Pending Approval
              </Typography>
              <DataGrid
                autoHeight
                rows={pendingQuery.data ?? []}
                columns={pendingColumns}
                loading={pendingQuery.isLoading}
                disableRowSelectionOnClick
                pageSizeOptions={[10, 20]}
                initialState={{ pagination: { paginationModel: { pageSize: 10, page: 0 } } }}
                slots={{ noRowsOverlay: PendingEmpty }}
                sx={{ minHeight: 300 }}
              />
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {selectedEmployeeId && (
        <Card>
          <CardContent>
            <Typography variant="h6" mb={2}>
              Leave History — {employees.find((e) => e.id === selectedEmployeeId)?.name}
            </Typography>
            <DataGrid
              autoHeight
              rows={historyQuery.data ?? []}
              columns={historyColumns}
              loading={historyQuery.isLoading}
              disableRowSelectionOnClick
              pageSizeOptions={[10, 20]}
              slots={{ noRowsOverlay: HistoryEmpty }}
              sx={{ minHeight: 250 }}
            />
          </CardContent>
        </Card>
      )}

      <ApplyLeaveDialog
        open={applyOpen}
        onClose={() => setApplyOpen(false)}
        onSubmit={(data: ApplyLeaveForm) => applyMutation.mutate(data)}
        loading={applyMutation.isPending}
        employees={employees}
        tenantId={tenantId ?? ""}
      />

      <RejectLeaveDialog
        open={!!rejectOpen}
        onClose={() => setRejectOpen(null)}
        onConfirm={(note) => {
          if (rejectOpen) rejectMutation.mutate({ id: rejectOpen, note });
        }}
        loading={rejectMutation.isPending}
      />
    </Stack>
  );
}
