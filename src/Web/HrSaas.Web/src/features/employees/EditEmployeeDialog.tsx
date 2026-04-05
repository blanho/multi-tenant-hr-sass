import { zodResolver } from "@hookform/resolvers/zod";
import {
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Stack,
  TextField,
} from "@mui/material";
import { useEffect } from "react";
import { useForm } from "react-hook-form";
import type { EmployeeSummaryDto } from "./types";
import { editEmployeeSchema, type EditEmployeeForm } from "./schemas";

interface EditEmployeeDialogProps {
  open: boolean;
  employee: EmployeeSummaryDto | null;
  onClose: () => void;
  onSubmit: (data: EditEmployeeForm & { id: string }) => void;
  loading: boolean;
}

export function EditEmployeeDialog({
  open,
  employee,
  onClose,
  onSubmit,
  loading,
}: Readonly<EditEmployeeDialogProps>) {
  const form = useForm<EditEmployeeForm>({
    resolver: zodResolver(editEmployeeSchema),
  });

  useEffect(() => {
    if (employee) {
      form.reset({
        name: employee.name,
        department: employee.department,
        position: employee.position,
      });
    }
  }, [employee, form]);

  return (
    <Dialog open={open} onClose={onClose} fullWidth maxWidth="sm">
      <DialogTitle>Edit Employee</DialogTitle>
      <DialogContent>
        <Stack spacing={2} mt={1}>
          <TextField
            label="Full Name"
            error={!!form.formState.errors.name}
            helperText={form.formState.errors.name?.message}
            {...form.register("name")}
          />
          <TextField
            label="Department"
            error={!!form.formState.errors.department}
            helperText={form.formState.errors.department?.message}
            {...form.register("department")}
          />
          <TextField
            label="Position"
            error={!!form.formState.errors.position}
            helperText={form.formState.errors.position?.message}
            {...form.register("position")}
          />
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
        <Button
          variant="contained"
          onClick={form.handleSubmit((v) =>
            onSubmit({ id: employee!.id, ...v }),
          )}
          disabled={loading}
        >
          {loading ? "Saving..." : "Save Changes"}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
