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
import { useForm } from "react-hook-form";
import { createEmployeeSchema, type CreateEmployeeForm } from "./schemas";

interface CreateEmployeeDialogProps {
  open: boolean;
  onClose: () => void;
  onSubmit: (data: CreateEmployeeForm) => void;
  loading: boolean;
}

export function CreateEmployeeDialog({
  open,
  onClose,
  onSubmit,
  loading,
}: Readonly<CreateEmployeeDialogProps>) {
  const form = useForm<CreateEmployeeForm>({
    resolver: zodResolver(createEmployeeSchema),
    defaultValues: { name: "", department: "", position: "", email: "" },
  });

  const handleClose = () => {
    form.reset();
    onClose();
  };

  return (
    <Dialog open={open} onClose={handleClose} fullWidth maxWidth="sm">
      <DialogTitle>Create Employee</DialogTitle>
      <DialogContent>
        <Stack spacing={2} mt={1}>
          <TextField
            label="Full Name"
            error={!!form.formState.errors.name}
            helperText={form.formState.errors.name?.message}
            {...form.register("name")}
          />
          <TextField
            label="Email"
            type="email"
            error={!!form.formState.errors.email}
            helperText={form.formState.errors.email?.message}
            {...form.register("email")}
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
        <Button onClick={handleClose}>Cancel</Button>
        <Button
          variant="contained"
          onClick={form.handleSubmit(onSubmit)}
          disabled={loading}
        >
          {loading ? "Creating..." : "Create"}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
