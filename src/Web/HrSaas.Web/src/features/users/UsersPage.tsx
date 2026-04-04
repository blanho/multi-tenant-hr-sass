import PersonRoundedIcon from "@mui/icons-material/PersonRounded";
import {
  Box,
  Card,
  CardContent,
  Chip,
  Drawer,
  LinearProgress,
  Stack,
  Typography,
} from "@mui/material";
import type { GridColDef } from "@mui/x-data-grid";
import { DataGrid } from "@mui/x-data-grid";
import { useQuery } from "@tanstack/react-query";
import dayjs from "dayjs";
import { useCallback, useMemo, useState } from "react";
import { EmptyState } from "../../components/common/EmptyState";
import { PageHeader } from "../../components/common/PageHeader";
import { StatusChip } from "../../components/common/StatusChip";
import { useNotify } from "../../components/feedback/useNotify";
import { api } from "../../lib/api";
import { qk } from "../../lib/query-keys";
import type { UserDto } from "../../types/api";

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
  const [detailOpen, setDetailOpen] = useState(false);
  const [selected, setSelected] = useState<UserDto | null>(null);

  const usersQuery = useQuery({
    queryKey: qk.users.all,
    queryFn: api.users.list,
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
    ],
    [],
  );

  const rows = usersQuery.data ?? [];

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
    </Stack>
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
