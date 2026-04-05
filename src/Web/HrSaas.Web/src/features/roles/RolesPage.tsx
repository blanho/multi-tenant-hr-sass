import AddRoundedIcon from "@mui/icons-material/AddRounded";
import DeleteRoundedIcon from "@mui/icons-material/DeleteRounded";
import EditRoundedIcon from "@mui/icons-material/EditRounded";
import SecurityRoundedIcon from "@mui/icons-material/SecurityRounded";
import {
  Autocomplete,
  Box,
  Button,
  Card,
  CardContent,
  Checkbox,
  Chip,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  IconButton,
  LinearProgress,
  Stack,
  TextField,
  Tooltip,
  Typography,
} from "@mui/material";
import type { GridColDef } from "@mui/x-data-grid";
import { DataGrid } from "@mui/x-data-grid";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import dayjs from "dayjs";
import { useMemo, useState } from "react";
import { ConfirmDialog } from "../../components/common/ConfirmDialog";
import { EmptyState } from "../../components/common/EmptyState";
import { PageHeader } from "../../components/common/PageHeader";
import { useNotify } from "../../components/feedback/useNotify";
import { api } from "../../lib/api";
import { qk } from "../../lib/query-keys";
import type { RoleDto } from "../../types/api";

function RolesEmpty() {
  return (
    <EmptyState
      title="No Roles"
      description="Create a role to manage permissions"
      icon={<SecurityRoundedIcon sx={{ fontSize: 48 }} />}
    />
  );
}

export function RolesPage() {
  const notify = useNotify();
  const queryClient = useQueryClient();

  const [createOpen, setCreateOpen] = useState(false);
  const [roleName, setRoleName] = useState("");
  const [createPerms, setCreatePerms] = useState<string[]>([]);
  const [editOpen, setEditOpen] = useState(false);
  const [editRole, setEditRole] = useState<RoleDto | null>(null);
  const [editPerms, setEditPerms] = useState<string[]>([]);
  const [deleteTarget, setDeleteTarget] = useState<RoleDto | null>(null);

  const rolesQuery = useQuery({
    queryKey: qk.roles.all,
    queryFn: api.roles.list,
  });

  const availablePermsQuery = useQuery({
    queryKey: qk.roles.availablePermissions,
    queryFn: api.roles.getAvailablePermissions,
  });

  const createMutation = useMutation({
    mutationFn: () => api.roles.create({ name: roleName, permissions: createPerms }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: qk.roles.all });
      setCreateOpen(false);
      setRoleName("");
      setCreatePerms([]);
      notify.success("Role created");
    },
    onError: notify.error,
  });

  const updatePermsMutation = useMutation({
    mutationFn: () => {
      if (!editRole) return Promise.reject(new Error("No role selected"));
      return api.roles.updatePermissions(editRole.id, { permissions: editPerms });
    },
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: qk.roles.all });
      setEditOpen(false);
      notify.success("Permissions updated");
    },
    onError: notify.error,
  });

  const deleteMutation = useMutation({
    mutationFn: () => {
      if (!deleteTarget) return Promise.reject(new Error("No role selected"));
      return api.roles.delete(deleteTarget.id);
    },
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: qk.roles.all });
      setDeleteTarget(null);
      notify.success("Role deleted");
    },
    onError: notify.error,
  });

  const openEditPerms = (role: RoleDto) => {
    setEditRole(role);
    setEditPerms([...role.permissions]);
    setEditOpen(true);
  };

  const allPerms = availablePermsQuery.data ?? [];

  const columns = useMemo<GridColDef<RoleDto>[]>(
    () => [
      {
        field: "name",
        headerName: "Role Name",
        flex: 1,
        minWidth: 180,
        renderCell: ({ row }) => (
          <Stack direction="row" spacing={1} alignItems="center">
            <Typography variant="body2" fontWeight={600}>
              {row.name}
            </Typography>
            {row.isSystem && <Chip label="System" size="small" color="info" />}
          </Stack>
        ),
      },
      {
        field: "permissions",
        headerName: "Permissions",
        flex: 1,
        minWidth: 200,
        renderCell: ({ value }) => (
          <Box sx={{ display: "flex", flexWrap: "wrap", gap: 0.5 }}>
            {(value as string[]).slice(0, 3).map((p) => (
              <Chip key={p} label={p} size="small" variant="outlined" />
            ))}
            {(value as string[]).length > 3 && (
              <Chip
                label={`+${(value as string[]).length - 3}`}
                size="small"
                color="default"
              />
            )}
          </Box>
        ),
      },
      {
        field: "createdAt",
        headerName: "Created",
        width: 150,
        valueFormatter: (value: string) => dayjs(value).format("MMM D, YYYY"),
      },
      {
        field: "actions",
        headerName: "",
        width: 100,
        sortable: false,
        filterable: false,
        renderCell: ({ row }) => (
          <Stack direction="row" spacing={0.5}>
            <Tooltip title="Edit Permissions">
              <IconButton size="small" onClick={() => openEditPerms(row)}>
                <EditRoundedIcon fontSize="small" />
              </IconButton>
            </Tooltip>
            {!row.isSystem && (
              <Tooltip title="Delete Role">
                <IconButton
                  size="small"
                  color="error"
                  onClick={() => setDeleteTarget(row)}
                >
                  <DeleteRoundedIcon fontSize="small" />
                </IconButton>
              </Tooltip>
            )}
          </Stack>
        ),
      },
    ],
    [],
  );

  const rows = rolesQuery.data ?? [];

  return (
    <Stack spacing={2.5}>
      <PageHeader
        title="Roles & Permissions"
        subtitle="Manage roles, assign permissions, and control access"
        actions={
          <Button
            variant="contained"
            startIcon={<AddRoundedIcon />}
            onClick={() => setCreateOpen(true)}
          >
            Create Role
          </Button>
        }
      />

      {rolesQuery.isFetching && <LinearProgress />}

      <Card>
        <CardContent>
          <Box sx={{ height: 480 }}>
            <DataGrid
              rows={rows}
              columns={columns}
              loading={rolesQuery.isFetching}
              disableRowSelectionOnClick
              slots={{ noRowsOverlay: RolesEmpty }}
              slotProps={{ noRowsOverlay: {} }}
              density="compact"
              sx={{ border: 0 }}
            />
          </Box>
        </CardContent>
      </Card>

      <Dialog open={createOpen} onClose={() => setCreateOpen(false)} fullWidth maxWidth="sm">
        <DialogTitle>Create Role</DialogTitle>
        <DialogContent>
          <Stack spacing={2} mt={1}>
            <TextField
              autoFocus
              fullWidth
              label="Role Name"
              value={roleName}
              onChange={(e) => setRoleName(e.target.value)}
            />
            <Autocomplete
              multiple
              options={allPerms}
              value={createPerms}
              onChange={(_, v) => setCreatePerms(v)}
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
                <TextField {...params} label="Permissions" placeholder="Select permissions" />
              )}
              limitTags={5}
              slotProps={{ chip: { size: "small" } }}
              loading={availablePermsQuery.isFetching}
            />
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setCreateOpen(false)}>Cancel</Button>
          <Button
            variant="contained"
            onClick={() => createMutation.mutate()}
            disabled={createMutation.isPending || !roleName.trim()}
          >
            {createMutation.isPending ? "Creating..." : "Create"}
          </Button>
        </DialogActions>
      </Dialog>

      <Dialog open={editOpen} onClose={() => setEditOpen(false)} fullWidth maxWidth="sm">
        <DialogTitle>Edit Permissions — {editRole?.name}</DialogTitle>
        <DialogContent>
          <Stack spacing={2} mt={1}>
            <Typography variant="body2" color="text.secondary">
              Select permissions for this role from the available list
            </Typography>
            <Autocomplete
              multiple
              options={allPerms}
              value={editPerms}
              onChange={(_, v) => setEditPerms(v)}
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
                <TextField {...params} label="Permissions" placeholder="Select permissions" />
              )}
              limitTags={5}
              slotProps={{ chip: { size: "small" } }}
              loading={availablePermsQuery.isFetching}
            />
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setEditOpen(false)}>Cancel</Button>
          <Button
            variant="contained"
            onClick={() => updatePermsMutation.mutate()}
            disabled={updatePermsMutation.isPending}
          >
            {updatePermsMutation.isPending ? "Saving..." : "Save Permissions"}
          </Button>
        </DialogActions>
      </Dialog>

      <ConfirmDialog
        open={!!deleteTarget}
        title="Delete Role"
        message={`Are you sure you want to delete the role "${deleteTarget?.name}"? This action cannot be undone.`}
        confirmLabel="Delete"
        severity="error"
        loading={deleteMutation.isPending}
        onConfirm={() => deleteMutation.mutate()}
        onCancel={() => setDeleteTarget(null)}
      />
    </Stack>
  );
}
