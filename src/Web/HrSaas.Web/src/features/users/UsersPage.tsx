import PersonRoundedIcon from "@mui/icons-material/PersonRounded";
import SwapHorizRoundedIcon from "@mui/icons-material/SwapHorizRounded";
import {
  Box,
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
import { useCallback, useMemo, useState } from "react";
import { EmptyState, PageHeader, StatusChip } from "@/components";
import { useNotify } from "@/hooks/useNotify";
import { api } from "@/lib/api";
import { qk } from "@/lib/query-keys";
import type { RoleDto, UserDto } from "@/types/api";
import { UserDetailDrawer } from "./UserDetailDrawer";
import { AssignRoleDialog } from "./AssignRoleDialog";

function UsersEmpty() {
  return (
    <EmptyState
      title="No Users"
      description="No users have been registered for this tenant yet"
      icon={<PersonRoundedIcon sx={{ fontSize: 48 }} />}
    />
  );
}

export function UsersPage() {
  const notify = useNotify();
  const queryClient = useQueryClient();
  const [detailOpen, setDetailOpen] = useState(false);
  const [selected, setSelected] = useState<UserDto | null>(null);
  const [assignUser, setAssignUser] = useState<UserDto | null>(null);

  const usersQuery = useQuery({
    queryKey: qk.users.all,
    queryFn: api.users.list,
  });

  const rolesQuery = useQuery({
    queryKey: qk.roles.all,
    queryFn: api.roles.list,
  });

  const assignRoleMutation = useMutation({
    mutationFn: ({ userId, roleId }: { userId: string; roleId: string }) =>
      api.roles.assign({ userId, roleId }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: qk.users.all });
      setAssignUser(null);
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

  const columns = useMemo<GridColDef<UserDto>[]>(
    () => [
      { field: "email", headerName: "Email", flex: 1, minWidth: 220 },
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
            {(value as string[]).length} permission
            {(value as string[]).length === 1 ? "" : "s"}
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
                setAssignUser(row);
              }}
            >
              <SwapHorizRoundedIcon fontSize="small" />
            </IconButton>
          </Tooltip>
        ),
      },
    ],
    [],
  );

  const rows = usersQuery.data ?? [];
  const roles: RoleDto[] = rolesQuery.data ?? [];

  return (
    <Stack spacing={2.5}>
      <PageHeader title="Users" subtitle="Manage registered users and their roles" />

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

      <UserDetailDrawer
        open={detailOpen}
        onClose={() => setDetailOpen(false)}
        user={selected}
        onChangeRole={(user) => setAssignUser(user)}
      />

      <AssignRoleDialog
        open={!!assignUser}
        user={assignUser}
        roles={roles}
        onClose={() => setAssignUser(null)}
        onSubmit={(userId, roleId) => assignRoleMutation.mutate({ userId, roleId })}
        loading={assignRoleMutation.isPending}
      />
    </Stack>
  );
}
