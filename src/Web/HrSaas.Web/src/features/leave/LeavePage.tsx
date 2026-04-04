import {
  Alert,
  Button,
  Card,
  CardContent,
  Chip,
  Grid,
  MenuItem,
  Stack,
  TextField,
  Typography,
} from "@mui/material";
import { DataGrid, type GridColDef } from "@mui/x-data-grid";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useMemo, useState } from "react";
import dayjs from "dayjs";
import { PageHeader } from "../../components/common/PageHeader";
import { api } from "../../lib/api";
import { extractErrorMessage } from "../../lib/http";
import { useAuth } from "../auth/auth-context";
import type { LeaveRequestDto } from "../../types/api";

const leaveTypes = ["Annual", "Sick", "Unpaid"] as const;

export function LeavePage() {
  const [employeeId, setEmployeeId] = useState("");
  const [type, setType] = useState<(typeof leaveTypes)[number]>("Annual");
  const [startDate, setStartDate] = useState(dayjs().add(1, "day").format("YYYY-MM-DD"));
  const [endDate, setEndDate] = useState(dayjs().add(1, "day").format("YYYY-MM-DD"));
  const [reason, setReason] = useState("");

  const { tenantId } = useAuth();
  const queryClient = useQueryClient();

  const pendingQuery = useQuery({
    queryKey: ["leave", "pending"],
    queryFn: api.getPendingLeaves,
  });

  const balanceQuery = useQuery({
    queryKey: ["leave", "balance", employeeId],
    queryFn: () => api.getLeaveBalance(employeeId),
    enabled: employeeId.length > 0,
  });

  const applyMutation = useMutation({
    mutationFn: api.applyLeave,
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ["leave"] });
      setReason("");
    },
  });

  const approveMutation = useMutation({
    mutationFn: api.approveLeave,
    onSuccess: () => void queryClient.invalidateQueries({ queryKey: ["leave"] }),
  });

  const rejectMutation = useMutation({
    mutationFn: (leaveId: string) => api.rejectLeave(leaveId, "Rejected from dashboard"),
    onSuccess: () => void queryClient.invalidateQueries({ queryKey: ["leave"] }),
  });

  const columns = useMemo<GridColDef<LeaveRequestDto>[]>(
    () => [
      { field: "employeeId", headerName: "Employee ID", minWidth: 220, flex: 1 },
      { field: "type", headerName: "Type", width: 120 },
      {
        field: "durationDays",
        headerName: "Days",
        width: 90,
      },
      {
        field: "status",
        headerName: "Status",
        width: 130,
        renderCell: ({ value }) => <Chip size="small" label={String(value)} />,
      },
      {
        field: "actions",
        headerName: "Actions",
        minWidth: 220,
        sortable: false,
        renderCell: ({ row }) => (
          <Stack direction="row" spacing={1}>
            <Button size="small" variant="outlined" onClick={() => approveMutation.mutate(row.id)}>
              Approve
            </Button>
            <Button size="small" color="error" onClick={() => rejectMutation.mutate(row.id)}>
              Reject
            </Button>
          </Stack>
        ),
      },
    ],
    [approveMutation, rejectMutation],
  );

  return (
    <Stack spacing={2.5}>
      <PageHeader
        title="Leave Management"
        subtitle="Apply, approve, reject, and monitor leave balances with tenant-level isolation"
      />

      {(pendingQuery.isError || applyMutation.isError || approveMutation.isError) && (
        <Alert severity="error">
          {extractErrorMessage(
            pendingQuery.error ?? applyMutation.error ?? approveMutation.error,
          )}
        </Alert>
      )}

      <Grid container spacing={2}>
        <Grid size={{ xs: 12, md: 8 }}>
          <Card>
            <CardContent>
              <Typography variant="h6" mb={2}>
                Apply Leave
              </Typography>
              <Grid container spacing={2}>
                <Grid size={{ xs: 12, md: 6 }}>
                  <TextField
                    fullWidth
                    label="Employee ID"
                    value={employeeId}
                    onChange={(e) => setEmployeeId(e.target.value)}
                  />
                </Grid>
                <Grid size={{ xs: 12, md: 6 }}>
                  <TextField fullWidth select label="Leave Type" value={type} onChange={(e) => setType(e.target.value as (typeof leaveTypes)[number])}>
                    {leaveTypes.map((item) => (
                      <MenuItem key={item} value={item}>
                        {item}
                      </MenuItem>
                    ))}
                  </TextField>
                </Grid>
                <Grid size={{ xs: 12, md: 6 }}>
                  <TextField
                    fullWidth
                    type="date"
                    label="Start Date"
                    value={startDate}
                    onChange={(e) => setStartDate(e.target.value)}
                    slotProps={{ inputLabel: { shrink: true } }}
                  />
                </Grid>
                <Grid size={{ xs: 12, md: 6 }}>
                  <TextField
                    fullWidth
                    type="date"
                    label="End Date"
                    value={endDate}
                    onChange={(e) => setEndDate(e.target.value)}
                    slotProps={{ inputLabel: { shrink: true } }}
                  />
                </Grid>
                <Grid size={{ xs: 12 }}>
                  <TextField
                    fullWidth
                    multiline
                    minRows={2}
                    label="Reason"
                    value={reason}
                    onChange={(e) => setReason(e.target.value)}
                  />
                </Grid>
                <Grid size={{ xs: 12 }}>
                  <Button
                    variant="contained"
                    onClick={() => {
                      if (!tenantId || !employeeId || !reason) return;
                      applyMutation.mutate({
                        tenantId,
                        employeeId,
                        type,
                        startDate,
                        endDate,
                        reason,
                      });
                    }}
                    disabled={applyMutation.isPending || !tenantId || !employeeId || !reason}
                  >
                    Submit Leave Request
                  </Button>
                </Grid>
              </Grid>
            </CardContent>
          </Card>
        </Grid>

        <Grid size={{ xs: 12, md: 4 }}>
          <Card>
            <CardContent>
              <Typography variant="h6" mb={2}>
                Balance
              </Typography>
              <Typography variant="body2" color="text.secondary">
                Annual Remaining: {balanceQuery.data?.annualRemaining ?? "-"}
              </Typography>
              <Typography variant="body2" color="text.secondary">
                Sick Remaining: {balanceQuery.data?.sickRemaining ?? "-"}
              </Typography>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      <Card>
        <CardContent>
          <Typography variant="h6" mb={2}>
            Pending Requests
          </Typography>
          <DataGrid
            autoHeight
            rows={pendingQuery.data ?? []}
            columns={columns}
            loading={pendingQuery.isLoading}
            disableRowSelectionOnClick
            pageSizeOptions={[10, 20]}
          />
        </CardContent>
      </Card>
    </Stack>
  );
}
