import AddRoundedIcon from "@mui/icons-material/AddRounded";
import DeleteRoundedIcon from "@mui/icons-material/DeleteRounded";
import EditRoundedIcon from "@mui/icons-material/EditRounded";
import SecurityRoundedIcon from "@mui/icons-material/SecurityRounded";
import {
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  IconButton,
  LinearProgress,
  Stack,
  Tooltip,
  Typography,
} from "@mui/material";
import type { GridColDef } from "@mui/x-data-grid";
import { DataGrid } from "@mui/x-data-grid";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import dayjs from "dayjs";
import { useMemo, useState } from "react";
import { ConfirmDialog, EmptyState, PageHeader } from "@/components";
import { useNotify } from "@/hooks/useNotify";
import { qk } from "@/lib/query-keys";
import { rolesApi } from "./api";
import type { RoleDto } from "./types";
import type { CreateRoleForm } from "./schemas";
import { CreateRoleDialog } from "./CreateRoleDialog";
import { EditPermissionsDialog } from "./EditPermissionsDialog";

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
  const [editRole, setEditRole] = useState<RoleDto | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<RoleDto | null>(null);

  const rolesQuery = useQuery({
    queryKey: qk.roles.all,
    queryFn: rolesApi.list,
  });

  const availablePermsQuery = useQuery({
    queryKey: qk.roles.availablePermissions,
    queryFn: rolesApi.getAvailablePermissions,
  });

  const createMutation = useMutation({
    mutationFn: (data: CreateRoleForm) =>
      rolesApi.create({ name: data.name, permissions: data.permissions }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: qk.roles.all });
      setCreateOpen(false);
      notify.success("Role created");
    },
    onError: notify.error,
  });

  const updatePermsMutation = useMutation({
    mutationFn: (perms: string[]) => {
      if (!editRole) return Promise.reject(new Error("No role selected"));
      return rolesApi.updatePermissions(editRole.id, { permissions: perms });
    },
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: qk.roles.all });
      setEditRole(null);
      notify.success("Permissions updated");
    },
    onError: notify.error,
  });

  const deleteMutation = useMutation({
    mutationFn: () => {
      if (!deleteTarget) return Promise.reject(new Error("No role selected"));
      return rolesApi.delete(deleteTarget.id);
    },
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: qk.roles.all });
      setDeleteTarget(null);
      notify.success("Role deleted");
    },
    onError: notify.error,
  });

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
              <IconButton size="small" onClick={() => setEditRole(row)}>
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

      <CreateRoleDialog
        open={createOpen}
        onClose={() => setCreateOpen(false)}
        onSubmit={(data) => createMutation.mutate(data)}
        loading={createMutation.isPending}
        availablePermissions={availablePermsQuery.data ?? []}
        permissionsLoading={availablePermsQuery.isFetching}
      />

      <EditPermissionsDialog
        open={!!editRole}
        role={editRole}
        onClose={() => setEditRole(null)}
        onSubmit={(perms) => updatePermsMutation.mutate(perms)}
        loading={updatePermsMutation.isPending}
        availablePermissions={availablePermsQuery.data ?? []}
        permissionsLoading={availablePermsQuery.isFetching}
      />

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
