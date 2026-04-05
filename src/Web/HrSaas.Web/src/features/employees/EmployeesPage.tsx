import AddRoundedIcon from "@mui/icons-material/AddRounded";
import DeleteOutlineRoundedIcon from "@mui/icons-material/DeleteOutlineRounded";
import EditRoundedIcon from "@mui/icons-material/EditRounded";
import SearchRoundedIcon from "@mui/icons-material/SearchRounded";
import {
  Button,
  IconButton,
  InputAdornment,
  Stack,
  TextField,
  Tooltip,
} from "@mui/material";
import { DataGrid, type GridColDef, type GridPaginationModel } from "@mui/x-data-grid";
import { useMemo, useState } from "react";
import { ConfirmDialog, EmptyState, PageHeader } from "@/components";
import type { EmployeeSummaryDto } from "./types";
import type { CreateEmployeeForm, EditEmployeeForm } from "./schemas";
import {
  useCreateEmployee,
  useDeleteEmployee,
  useEmployeeList,
  useUpdateEmployee,
} from "./hooks";
import { CreateEmployeeDialog } from "./CreateEmployeeDialog";
import { EditEmployeeDialog } from "./EditEmployeeDialog";

export function EmployeesPage() {
  const [search, setSearch] = useState("");
  const [paginationModel, setPaginationModel] = useState<GridPaginationModel>({
    page: 0,
    pageSize: 20,
  });
  const [createOpen, setCreateOpen] = useState(false);
  const [editTarget, setEditTarget] = useState<EmployeeSummaryDto | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<EmployeeSummaryDto | null>(null);

  const department = search.trim() || undefined;
  const page = paginationModel.page + 1;
  const pageSize = paginationModel.pageSize;

  const employeesQuery = useEmployeeList(page, pageSize, department);
  const createMutation = useCreateEmployee(() => setCreateOpen(false));
  const editMutation = useUpdateEmployee(() => setEditTarget(null));
  const deleteMutation = useDeleteEmployee();

  const columns = useMemo<GridColDef<EmployeeSummaryDto>[]>(
    () => [
      { field: "name", headerName: "Name", flex: 1, minWidth: 180 },
      { field: "department", headerName: "Department", flex: 1, minWidth: 140 },
      { field: "position", headerName: "Position", flex: 1, minWidth: 140 },
      {
        field: "actions",
        headerName: "Actions",
        width: 120,
        sortable: false,
        renderCell: ({ row }) => (
          <Stack direction="row" spacing={0.5}>
            <Tooltip title="Edit">
              <IconButton size="small" onClick={() => setEditTarget(row)}>
                <EditRoundedIcon fontSize="small" />
              </IconButton>
            </Tooltip>
            <Tooltip title="Delete">
              <IconButton size="small" color="error" onClick={() => setDeleteTarget(row)}>
                <DeleteOutlineRoundedIcon fontSize="small" />
              </IconButton>
            </Tooltip>
          </Stack>
        ),
      },
    ],
    [],
  );

  return (
    <Stack spacing={2.5}>
      <PageHeader
        title="Employees"
        subtitle="Manage employee directory with tenant-isolated records"
        actions={
          <Button
            startIcon={<AddRoundedIcon />}
            variant="contained"
            onClick={() => setCreateOpen(true)}
          >
            Add Employee
          </Button>
        }
      />

      <TextField
        placeholder="Filter by department..."
        size="small"
        value={search}
        onChange={(e) => setSearch(e.target.value)}
        slotProps={{
          input: {
            startAdornment: (
              <InputAdornment position="start">
                <SearchRoundedIcon fontSize="small" />
              </InputAdornment>
            ),
          },
        }}
        sx={{ maxWidth: 360 }}
      />

      <DataGrid
        autoHeight
        rows={employeesQuery.data?.items ?? []}
        columns={columns}
        rowCount={employeesQuery.data?.totalCount ?? 0}
        loading={employeesQuery.isFetching}
        paginationMode="server"
        paginationModel={paginationModel}
        onPaginationModelChange={setPaginationModel}
        pageSizeOptions={[10, 20, 50]}
        disableRowSelectionOnClick
        slots={{
          noRowsOverlay: () => (
            <EmptyState
              title="No employees found"
              description={
                department
                  ? `No employees in "${department}" department`
                  : "Add your first employee to get started"
              }
            />
          ),
        }}
        sx={{ minHeight: 400 }}
      />

      <CreateEmployeeDialog
        open={createOpen}
        onClose={() => setCreateOpen(false)}
        onSubmit={(data: CreateEmployeeForm) => createMutation.mutate(data)}
        loading={createMutation.isPending}
      />

      <EditEmployeeDialog
        open={!!editTarget}
        employee={editTarget}
        onClose={() => setEditTarget(null)}
        onSubmit={(data: EditEmployeeForm & { id: string }) =>
          editMutation.mutate({ id: data.id, payload: data })
        }
        loading={editMutation.isPending}
      />

      <ConfirmDialog
        open={!!deleteTarget}
        title="Delete Employee"
        message={`Are you sure you want to permanently delete "${deleteTarget?.name}"? This action cannot be undone.`}
        confirmLabel="Delete"
        severity="error"
        loading={deleteMutation.isPending}
        onConfirm={() => deleteTarget && deleteMutation.mutate(deleteTarget.id)}
        onCancel={() => setDeleteTarget(null)}
      />
    </Stack>
  );
}
