import PersonRoundedIcon from "@mui/icons-material/PersonRounded";
import SwapHorizRoundedIcon from "@mui/icons-material/SwapHorizRounded";
import {
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Drawer,
  FormControl,
  IconButton,
  InputLabel,
  LinearProgress,
  MenuItem,
  Select,
  Stack,
  Tooltip,
  Typography,
} from "@mui/material";
import type { GridColDef } from "@mui/x-data-grid";
import { DataGrid } from "@mui/x-data-grid";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import dayjs from "dayjs";
import { useCallback, useMemo, useState } from "react";
import { EmptyState } from "../../components/common/EmptyState";
import { PageHeader } from "../../components/common/PageHeader";
import { StatusChip } from "../../components/common/StatusChip";
import { useNotify } from "../../components/feedback/useNotify";
import { api } from "../../lib/api";
import { qk } from "../../lib/query-keys";
import type { RoleDto, UserDto } from "../../types/api";

function UsersEmpty() {
  return (
    <EmptyState
      title="No Users"
      description="No users have been registered for this tenant yet"
      icon={<PersonRoundedIcon sx={{ fontSize: 48 }} />}
    />
  );
}

function DetailRow({ label, value }: Readonly<{ label: string; value: string }>) {
  return (
    <Stack direction="row" justifyContent="space-between" py={0.5}>
      <Typography variant="body2" color="text.secondary">
        {label}
      </Typography>
      <Typography variant="body2" fontWeight={600} sx={{ maxWidth: 200, wordBreak: "break-all" }}>
        {value}
      </Typography>
    </Stack>
  );
}

export function UsersPage() {
  const notify = useNotify();
  const queryClient = useQueryClient();
  const [detailOpen, setDetailOpen] = useState(false);
  const [selected, setSelected] = useState<UserDto | null>(null);
  const [assignOpen, setAssignOpen] = useState(false);
  const [assignUser, setAssignUser] = useState<UserDto | null>(null);
  const [selectedRoleId, setSelectedRoleId] = useState("");

  const usersQuery = useQuery({
    queryKey: qk.users.all,
    queryFn: api.users.list,
  });

  const rolesQuery = useQuery({
    queryKey: qk.roles.all,
    queryFn: api.roles.list,
  });

  const assignRoleMutation = useMutation({
    mutationFn: () => {
      if (!assignUser || !selectedRoleId) return Promise.reject(new Error("Missing data"));
      return api.roles.assign({ userId: assignUser.id, roleId: selectedRoleId });
    },
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: qk.users.all });
      setAssignOpen(false);
      setAssignUser(null);
      setSelectedRoleId("");
      notify.success("Role assigned successfully");
    },
    onError: notify.error,
  });

  const openDetail = useCallback(
    async (id: string) => {
      try {
        const user = await api.users.getById(id);
        setSelected(user);
        setDetailOpen(true);
      } catch {
        notify.error("Failed to load user details");
      }
    },
    [notify],
  );

  const openAssignRole = useCallback((user: UserDto) => {
    setAssignUser(user);
    setSelectedRoleId(user.roleId);
    setAssignOpen(true);
  }, []);

  const columns = useMemo<GridColDef<UserDto>[]>(
    () => [
      {
        field: "email",
        headerName: "Email",
        flex: 1,
        minWidth: 220,
      },
      {
        field: "roleName",
        headerName: "Role",
        width: 140,
        renderCell: ({ value }) => <Chip label={value} size="small" variant="outlined" />,
      },
      {
        field: "isActive",
        headerName: "Status",
        width: 110,
        renderCell: ({ value }) => (
          <StatusChip status={value ? "Active" : "Suspended"} />
        ),
      },
      {
        field: "createdAt",
        headerName: "Joined",
        width: 150,
        valueFormatter: (value: string) => dayjs(value).format("MMM D, YYYY"),
      },
      {
        field: "permissions",
        headerName: "Permissions",
        width: 130,
        renderCell: ({ value }) => (
          <Typography variant="body2" color="text.secondary">
            {(value as string[]).length} permission{(value as string[]).length === 1 ? "" : "s"}
          </Typography>
        ),
      },
      {
        field: "actions",
        headerName: "",
        width: 60,
        sortable: false,
        filterable: false,
        renderCell: ({ row }) => (
          <Tooltip title="Assign Role">
            <IconButton
              size="small"
              onClick={(e) => {
                e.stopPropagation();
                openAssignRole(row);
              }}
            >
              <SwapHorizRoundedIcon fontSize="small" />
            </IconButton>
          </Tooltip>
        ),
      },
    ],
    [openAssignRole],
  );

  const rows = usersQuery.data ?? [];
  const roles: RoleDto[] = rolesQuery.data ?? [];

  return (
    <Stack spacing={2.5}>
      <PageHeader
        title="Users"
        subtitle="Manage registered users and their roles"
      />

      {usersQuery.isFetching && <LinearProgress />}

      <Card>
        <CardContent>
          <Box sx={{ height: 520 }}>
            <DataGrid
              rows={rows}
              columns={columns}
              loading={usersQuery.isFetching}
              disableRowSelectionOnClick
              onRowClick={({ row }) => void openDetail(row.id)}
              slots={{ noRowsOverlay: UsersEmpty }}
              slotProps={{ noRowsOverlay: {} }}
              density="compact"
              sx={{
                border: 0,
                "& .MuiDataGrid-row": { cursor: "pointer" },
              }}
            />
          </Box>
        </CardContent>
      </Card>

      <Drawer
        anchor="right"
        open={detailOpen}
        onClose={() => setDetailOpen(false)}
        slotProps={{ paper: { sx: { width: 380, p: 3 } } }}
      >
        {selected && (
          <Stack spacing={2}>
            <Typography variant="h6">User Details</Typography>
            <StatusChip status={selected.isActive ? "Active" : "Suspended"} />
            <Stack spacing={1} mt={1}>
              <DetailRow label="Email" value={selected.email} />
              <DetailRow label="Role" value={selected.roleName} />
              <DetailRow label="Joined" value={dayjs(selected.createdAt).format("MMM D, YYYY")} />
              <DetailRow label="User ID" value={selected.id} />
              <DetailRow label="Role ID" value={selected.roleId} />
            </Stack>
            <Button
              variant="outlined"
              size="small"
              startIcon={<SwapHorizRoundedIcon />}
              onClick={() => {
                setDetailOpen(false);
                openAssignRole(selected);
              }}
            >
              Change Role
            </Button>
            <Typography variant="subtitle2" mt={2}>
              Permissions ({selected.permissions.length})
            </Typography>
            <Box sx={{ display: "flex", flexWrap: "wrap", gap: 0.5 }}>
              {selected.permissions.map((p) => (
                <Chip key={p} label={p} size="small" variant="outlined" />
              ))}
              {selected.permissions.length === 0 && (
                <Typography variant="body2" color="text.secondary">
                  No permissions assigned
                </Typography>
              )}
            </Box>
          </Stack>
        )}
      </Drawer>

      <Dialog open={assignOpen} onClose={() => setAssignOpen(false)} fullWidth maxWidth="xs">
        <DialogTitle>Assign Role</DialogTitle>
        <DialogContent>
          {assignUser && (
            <Stack spacing={2} mt={1}>
              <Typography variant="body2" color="text.secondary">
                Change role for <strong>{assignUser.email}</strong>
              </Typography>
              <Typography variant="caption" color="text.disabled">
                Current role: {assignUser.roleName}
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
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setAssignOpen(false)}>Cancel</Button>
          <Button
            variant="contained"
            onClick={() => assignRoleMutation.mutate()}
            disabled={
              assignRoleMutation.isPending ||
              !selectedRoleId ||
              selectedRoleId === assignUser?.roleId
            }
          >
            {assignRoleMutation.isPending ? "Assigning..." : "Assign Role"}
          </Button>
        </DialogActions>
      </Dialog>
    </Stack>
  );
}
