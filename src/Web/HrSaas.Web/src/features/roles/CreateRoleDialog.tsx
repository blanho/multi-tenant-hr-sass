import { zodResolver } from "@hookform/resolvers/zod";
import {
  Autocomplete,
  Button,
  Checkbox,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Stack,
  TextField,
} from "@mui/material";
import { Controller, useForm } from "react-hook-form";
import { createRoleSchema, type CreateRoleForm } from "./schemas";

interface CreateRoleDialogProps {
  open: boolean;
  onClose: () => void;
  onSubmit: (data: CreateRoleForm) => void;
  loading: boolean;
  availablePermissions: string[];
  permissionsLoading: boolean;
}

export function CreateRoleDialog({
  open,
  onClose,
  onSubmit,
  loading,
  availablePermissions,
  permissionsLoading,
}: Readonly<CreateRoleDialogProps>) {
  const form = useForm<CreateRoleForm>({
    resolver: zodResolver(createRoleSchema),
    defaultValues: { name: "", permissions: [] },
  });

  const handleClose = () => {
    form.reset();
    onClose();
  };

  return (
    <Dialog open={open} onClose={handleClose} fullWidth maxWidth="sm">
      <DialogTitle>Create Role</DialogTitle>
      <DialogContent>
        <Stack spacing={2} mt={1}>
          <TextField
            autoFocus
            fullWidth
            label="Role Name"
            error={!!form.formState.errors.name}
            helperText={form.formState.errors.name?.message}
            {...form.register("name")}
          />
          <Controller
            name="permissions"
            control={form.control}
            render={({ field }) => (
              <Autocomplete
                multiple
                options={availablePermissions}
                value={field.value}
                onChange={(_, v) => field.onChange(v)}
                disableCloseOnSelect
                renderOption={(props, option, { selected: checked }) => {
                  const { key, ...rest } = props;
                  return (
                    <li key={key} {...rest}>
                      <Checkbox size="small" checked={checked} sx={{ mr: 1 }} />
                      {option}
                    </li>
                  );
                }}
                renderInput={(params) => (
                  <TextField
                    {...params}
                    label="Permissions"
                    placeholder="Select permissions"
                  />
                )}
                limitTags={5}
                slotProps={{ chip: { size: "small" } }}
                loading={permissionsLoading}
              />
            )}
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
