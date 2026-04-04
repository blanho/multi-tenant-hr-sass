import CheckCircleRoundedIcon from "@mui/icons-material/CheckCircleRounded";
import CancelRoundedIcon from "@mui/icons-material/CancelRounded";
import {
  Autocomplete,
  Button,
  Card,
  CardContent,
  Chip,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Grid,
  LinearProgress,
  MenuItem,
  Stack,
  TextField,
  Typography,
} from "@mui/material";
import { DataGrid, type GridColDef } from "@mui/x-data-grid";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import dayjs from "dayjs";
import { useMemo, useState } from "react";
import { EmptyState } from "../../components/common/EmptyState";
import { PageHeader } from "../../components/common/PageHeader";
import { StatusChip } from "../../components/common/StatusChip";
import { useNotify } from "../../components/feedback/useNotify";
import { api } from "../../lib/api";
import { qk } from "../../lib/query-keys";
import { useAuth } from "../auth/auth-context";
import type { LeaveRequestDto, LeaveType } from "../../types/api";

const LEAVE_TYPES: LeaveType[] = [
  "Annual",
  "Sick",
  "Maternity",
  "Paternity",
  "Unpaid",
  "Emergency",
];

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
  const notify = useNotify();
  const queryClient = useQueryClient();

  const [selectedEmployeeId, setSelectedEmployeeId] = useState<string | null>(null);
  const [applyOpen, setApplyOpen] = useState(false);
  const [rejectOpen, setRejectOpen] = useState<string | null>(null);
  const [rejectNote, setRejectNote] = useState("");

  const [leaveType, setLeaveType] = useState<LeaveType>("Annual");
  const [startDate, setStartDate] = useState(dayjs().add(1, "day").format("YYYY-MM-DD"));
  const [endDate, setEndDate] = useState(dayjs().add(1, "day").format("YYYY-MM-DD"));
  const [reason, setReason] = useState("");

  const employeesQuery = useQuery({
    queryKey: qk.employees.list(1, 200),
    queryFn: () => api.employees.list(1, 200),
  });

  const pendingQuery = useQuery({
    queryKey: qk.leave.pending,
    queryFn: api.leave.getPending,
  });

  const balanceQuery = useQuery({
    queryKey: qk.leave.balance(selectedEmployeeId ?? "", dayjs().year()),
    queryFn: () => api.leave.getBalance(selectedEmployeeId ?? "", dayjs().year()),
    enabled: !!selectedEmployeeId,
  });

  const historyQuery = useQuery({
    queryKey: qk.leave.byEmployee(selectedEmployeeId ?? "", dayjs().year()),
    queryFn: () => api.leave.getByEmployee(selectedEmployeeId ?? "", dayjs().year()),
    enabled: !!selectedEmployeeId,
  });

  const applyMutation = useMutation({
    mutationFn: api.leave.apply,
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: qk.leave.all });
      setApplyOpen(false);
      setReason("");
      notify.success("Leave request submitted");
    },
    onError: notify.error,
  });

  const approveMutation = useMutation({
    mutationFn: api.leave.approve,
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: qk.leave.all });
      notify.success("Leave approved");
    },
    onError: notify.error,
  });

  const rejectMutation = useMutation({
    mutationFn: ({ id, note }: { id: string; note: string }) =>
      api.leave.reject(id, note),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: qk.leave.all });
      setRejectOpen(null);
      setRejectNote("");
      notify.success("Leave rejected");
    },
    onError: notify.error,
  });

  const cancelMutation = useMutation({
    mutationFn: api.leave.cancel,
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: qk.leave.all });
      notify.success("Leave cancelled");
    },
    onError: notify.error,
  });

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
            <Button
              size="small"
              color="error"
              onClick={() => cancelMutation.mutate(row.id)}
            >
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

      <Dialog open={applyOpen} onClose={() => setApplyOpen(false)} fullWidth maxWidth="sm">
        <DialogTitle>Apply for Leave</DialogTitle>
        <DialogContent>
          <Stack spacing={2} mt={1}>
            <Autocomplete
              options={employees}
              getOptionLabel={(o) => `${o.name} (${o.department})`}
              onChange={(_, v) => setSelectedEmployeeId(v?.id ?? null)}
              renderInput={(params) => (
                <TextField {...params} label="Employee" required />
              )}
            />
            <TextField
              select
              label="Leave Type"
              value={leaveType}
              onChange={(e) => setLeaveType(e.target.value as LeaveType)}
            >
              {LEAVE_TYPES.map((t) => (
                <MenuItem key={t} value={t}>
                  {t}
                </MenuItem>
              ))}
            </TextField>
            <Stack direction="row" spacing={2}>
              <TextField
                fullWidth
                type="date"
                label="Start Date"
                value={startDate}
                onChange={(e) => setStartDate(e.target.value)}
                slotProps={{ inputLabel: { shrink: true } }}
              />
              <TextField
                fullWidth
                type="date"
                label="End Date"
                value={endDate}
                onChange={(e) => setEndDate(e.target.value)}
                slotProps={{ inputLabel: { shrink: true } }}
              />
            </Stack>
            <TextField
              multiline
              minRows={3}
              label="Reason"
              value={reason}
              onChange={(e) => setReason(e.target.value)}
              required
            />
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setApplyOpen(false)}>Cancel</Button>
          <Button
            variant="contained"
            disabled={
              !selectedEmployeeId || !tenantId || !reason || applyMutation.isPending
            }
            onClick={() => {
              if (!selectedEmployeeId || !tenantId) return;
              applyMutation.mutate({
                tenantId,
                employeeId: selectedEmployeeId,
                type: leaveType,
                startDate,
                endDate,
                reason,
              });
            }}
          >
            {applyMutation.isPending ? "Submitting..." : "Submit Request"}
          </Button>
        </DialogActions>
      </Dialog>

      <Dialog
        open={!!rejectOpen}
        onClose={() => setRejectOpen(null)}
        fullWidth
        maxWidth="xs"
      >
        <DialogTitle>Reject Leave Request</DialogTitle>
        <DialogContent>
          <TextField
            fullWidth
            multiline
            minRows={2}
            label="Rejection reason"
            value={rejectNote}
            onChange={(e) => setRejectNote(e.target.value)}
            sx={{ mt: 1 }}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setRejectOpen(null)}>Cancel</Button>
          <Button
            variant="contained"
            color="error"
            disabled={!rejectNote || rejectMutation.isPending}
            onClick={() => {
              if (rejectOpen) {
                rejectMutation.mutate({ id: rejectOpen, note: rejectNote });
              }
            }}
          >
            Reject
          </Button>
        </DialogActions>
      </Dialog>
    </Stack>
  );
}
