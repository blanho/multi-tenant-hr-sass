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
import type { TenantDto } from "./types";
import { editTenantSchema, type EditTenantForm } from "./schemas";

interface EditTenantDialogProps {
  open: boolean;
  tenant: TenantDto | null;
  onClose: () => void;
  onSubmit: (data: EditTenantForm & { id: string }) => void;
  loading: boolean;
}

export function EditTenantDialog({
  open,
  tenant,
  onClose,
  onSubmit,
  loading,
}: Readonly<EditTenantDialogProps>) {
  const form = useForm<EditTenantForm>({
    resolver: zodResolver(editTenantSchema),
  });

  useEffect(() => {
    if (tenant) {
      form.reset({ name: tenant.name, contactEmail: tenant.contactEmail });
    }
  }, [tenant, form]);

  return (
    <Dialog open={open} onClose={onClose} fullWidth maxWidth="sm">
      <DialogTitle>Edit Tenant</DialogTitle>
      <DialogContent>
        <Stack spacing={2} mt={1}>
          <TextField
            label="Tenant Name"
            error={!!form.formState.errors.name}
            helperText={form.formState.errors.name?.message}
            {...form.register("name")}
          />
          <TextField
            label="Contact Email"
            type="email"
            error={!!form.formState.errors.contactEmail}
            helperText={form.formState.errors.contactEmail?.message}
            {...form.register("contactEmail")}
          />
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
        <Button
          variant="contained"
          onClick={form.handleSubmit((v) =>
            onSubmit({ id: tenant!.id, ...v }),
          )}
          disabled={loading}
        >
          Save
        </Button>
      </DialogActions>
    </Dialog>
  );
}
