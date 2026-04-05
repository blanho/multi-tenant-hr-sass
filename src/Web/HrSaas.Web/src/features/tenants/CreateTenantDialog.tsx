import { zodResolver } from "@hookform/resolvers/zod";
import {
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  MenuItem,
  Stack,
  TextField,
} from "@mui/material";
import { useForm } from "react-hook-form";
import { PLANS } from "./constants";
import { createTenantSchema, type CreateTenantForm } from "./schemas";

interface CreateTenantDialogProps {
  open: boolean;
  onClose: () => void;
  onSubmit: (data: CreateTenantForm) => void;
  loading: boolean;
}

export function CreateTenantDialog({
  open,
  onClose,
  onSubmit,
  loading,
}: Readonly<CreateTenantDialogProps>) {
  const form = useForm<CreateTenantForm>({
    resolver: zodResolver(createTenantSchema),
    defaultValues: { name: "", slug: "", contactEmail: "", plan: "Free" },
  });

  const handleClose = () => {
    form.reset();
    onClose();
  };

  return (
    <Dialog open={open} onClose={handleClose} fullWidth maxWidth="sm">
      <DialogTitle>Create Tenant</DialogTitle>
      <DialogContent>
        <Stack spacing={2} mt={1}>
          <TextField
            label="Tenant Name"
            error={!!form.formState.errors.name}
            helperText={form.formState.errors.name?.message}
            {...form.register("name")}
          />
          <TextField
            label="Slug"
            error={!!form.formState.errors.slug}
            helperText={
              form.formState.errors.slug?.message ??
              "URL-friendly identifier (lowercase, hyphens)"
            }
            {...form.register("slug")}
          />
          <TextField
            label="Contact Email"
            type="email"
            error={!!form.formState.errors.contactEmail}
            helperText={form.formState.errors.contactEmail?.message}
            {...form.register("contactEmail")}
          />
          <TextField
            select
            label="Plan"
            defaultValue="Free"
            {...form.register("plan")}
          >
            {PLANS.map((p) => (
              <MenuItem key={p} value={p}>
                {p}
              </MenuItem>
            ))}
          </TextField>
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
