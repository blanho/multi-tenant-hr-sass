import { zodResolver } from "@hookform/resolvers/zod";
import {
  Autocomplete,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  MenuItem,
  Stack,
  TextField,
} from "@mui/material";
import dayjs from "dayjs";
import { Controller, useForm } from "react-hook-form";
import type { EmployeeSummaryDto } from "@/types/api";
import { LEAVE_TYPES } from "./constants";
import { applyLeaveSchema, type ApplyLeaveForm } from "./schemas";

interface ApplyLeaveDialogProps {
  open: boolean;
  onClose: () => void;
  onSubmit: (data: ApplyLeaveForm) => void;
  loading: boolean;
  employees: EmployeeSummaryDto[];
  tenantId: string;
}

export function ApplyLeaveDialog({
  open,
  onClose,
  onSubmit,
  loading,
  employees,
  tenantId,
}: Readonly<ApplyLeaveDialogProps>) {
  const form = useForm<ApplyLeaveForm>({
    resolver: zodResolver(applyLeaveSchema),
    defaultValues: {
      employeeId: "",
      tenantId,
      type: "Annual",
      startDate: dayjs().add(1, "day").format("YYYY-MM-DD"),
      endDate: dayjs().add(1, "day").format("YYYY-MM-DD"),
      reason: "",
    },
  });

  const handleClose = () => {
    form.reset();
    onClose();
  };

  return (
    <Dialog open={open} onClose={handleClose} fullWidth maxWidth="sm">
      <DialogTitle>Apply for Leave</DialogTitle>
      <DialogContent>
        <Stack spacing={2} mt={1}>
          <Controller
            name="employeeId"
            control={form.control}
            render={({ field, fieldState }) => (
              <Autocomplete
                options={employees}
                getOptionLabel={(o) => `${o.name} (${o.department})`}
                value={employees.find((e) => e.id === field.value) ?? null}
                onChange={(_, v) => field.onChange(v?.id ?? "")}
                renderInput={(params) => (
                  <TextField
                    {...params}
                    label="Employee"
                    required
                    error={!!fieldState.error}
                    helperText={fieldState.error?.message}
                  />
                )}
              />
            )}
          />
          <Controller
            name="type"
            control={form.control}
            render={({ field }) => (
              <TextField select label="Leave Type" {...field}>
                {LEAVE_TYPES.map((t) => (
                  <MenuItem key={t} value={t}>
                    {t}
                  </MenuItem>
                ))}
              </TextField>
            )}
          />
          <Stack direction="row" spacing={2}>
            <TextField
              fullWidth
              type="date"
              label="Start Date"
              error={!!form.formState.errors.startDate}
              helperText={form.formState.errors.startDate?.message}
              slotProps={{ inputLabel: { shrink: true } }}
              {...form.register("startDate")}
            />
            <TextField
              fullWidth
              type="date"
              label="End Date"
              error={!!form.formState.errors.endDate}
              helperText={form.formState.errors.endDate?.message}
              slotProps={{ inputLabel: { shrink: true } }}
              {...form.register("endDate")}
            />
          </Stack>
          <TextField
            multiline
            minRows={3}
            label="Reason"
            required
            error={!!form.formState.errors.reason}
            helperText={form.formState.errors.reason?.message}
            {...form.register("reason")}
          />
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={handleClose}>Cancel</Button>
        <Button
          variant="contained"
          disabled={loading}
          onClick={form.handleSubmit(onSubmit)}
        >
          {loading ? "Submitting..." : "Submit Request"}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
