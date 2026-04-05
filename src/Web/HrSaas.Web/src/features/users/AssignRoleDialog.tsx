import {
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  Stack,
  Typography,
} from "@mui/material";
import { useState } from "react";
import type { RoleDto, UserDto } from "@/types/api";

interface AssignRoleDialogProps {
  open: boolean;
  user: UserDto | null;
  roles: RoleDto[];
  onClose: () => void;
  onSubmit: (userId: string, roleId: string) => void;
  loading: boolean;
}

export function AssignRoleDialog({
  open,
  user,
  roles,
  onClose,
  onSubmit,
  loading,
}: Readonly<AssignRoleDialogProps>) {
  return (
    <Dialog open={open} onClose={onClose} fullWidth maxWidth="xs">
      {user && (
        <AssignRoleForm
          key={user.id}
          user={user}
          roles={roles}
          onClose={onClose}
          onSubmit={onSubmit}
          loading={loading}
        />
      )}
    </Dialog>
  );
}

function AssignRoleForm({
  user,
  roles,
  onClose,
  onSubmit,
  loading,
}: Readonly<Omit<AssignRoleDialogProps, "open"> & { user: UserDto }>) {
  const [selectedRoleId, setSelectedRoleId] = useState(user.roleId);

  return (
    <>
      <DialogTitle>Assign Role</DialogTitle>
      <DialogContent>
        <Stack spacing={2} mt={1}>
          <Typography variant="body2" color="text.secondary">
            Change role for <strong>{user.email}</strong>
          </Typography>
          <Typography variant="caption" color="text.disabled">
            Current role: {user.roleName}
          </Typography>
          <FormControl fullWidth>
            <InputLabel>Role</InputLabel>
            <Select
              value={selectedRoleId}
              label="Role"
              onChange={(e) => setSelectedRoleId(e.target.value)}
            >
              {roles.map((r) => (
                <MenuItem key={r.id} value={r.id}>
                  {r.name}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
        <Button
          variant="contained"
          onClick={() => onSubmit(user.id, selectedRoleId)}
          disabled={loading || !selectedRoleId || selectedRoleId === user.roleId}
        >
          {loading ? "Assigning..." : "Assign Role"}
        </Button>
      </DialogActions>
    </>
  );
}
