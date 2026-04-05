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
  Typography,
} from "@mui/material";
import { useState } from "react";
import type { RoleDto } from "./types";

interface EditPermissionsDialogProps {
  open: boolean;
  role: RoleDto | null;
  onClose: () => void;
  onSubmit: (permissions: string[]) => void;
  loading: boolean;
  availablePermissions: string[];
  permissionsLoading: boolean;
}

export function EditPermissionsDialog({
  open,
  role,
  onClose,
  onSubmit,
  loading,
  availablePermissions,
  permissionsLoading,
}: Readonly<EditPermissionsDialogProps>) {
  return (
    <Dialog open={open} onClose={onClose} fullWidth maxWidth="sm">
      {role && (
        <EditPermissionsForm
          key={role.id}
          role={role}
          onClose={onClose}
          onSubmit={onSubmit}
          loading={loading}
          availablePermissions={availablePermissions}
          permissionsLoading={permissionsLoading}
        />
      )}
    </Dialog>
  );
}

function EditPermissionsForm({
  role,
  onClose,
  onSubmit,
  loading,
  availablePermissions,
  permissionsLoading,
}: Readonly<Omit<EditPermissionsDialogProps, "open"> & { role: RoleDto }>) {
  const [perms, setPerms] = useState<string[]>([...role.permissions]);

  return (
    <>
      <DialogTitle>Edit Permissions — {role.name}</DialogTitle>
      <DialogContent>
        <Stack spacing={2} mt={1}>
          <Typography variant="body2" color="text.secondary">
            Select permissions for this role from the available list
          </Typography>
          <Autocomplete
            multiple
            options={availablePermissions}
            value={perms}
            onChange={(_, v) => setPerms(v)}
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
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
        <Button
          variant="contained"
          onClick={() => onSubmit(perms)}
          disabled={loading || perms.length === 0}
        >
          {loading ? "Saving..." : "Save Permissions"}
        </Button>
      </DialogActions>
    </>
  );
}
