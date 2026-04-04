import { zodResolver } from "@hookform/resolvers/zod";
import AddRoundedIcon from "@mui/icons-material/AddRounded";
import DeleteOutlineRoundedIcon from "@mui/icons-material/DeleteOutlineRounded";
import EditRoundedIcon from "@mui/icons-material/EditRounded";
import SearchRoundedIcon from "@mui/icons-material/SearchRounded";
import {
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  IconButton,
  InputAdornment,
  Stack,
  TextField,
  Tooltip,
} from "@mui/material";
import { DataGrid, type GridColDef, type GridPaginationModel } from "@mui/x-data-grid";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useCallback, useMemo, useState } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { ConfirmDialog } from "../../components/common/ConfirmDialog";
import { EmptyState } from "../../components/common/EmptyState";
import { PageHeader } from "../../components/common/PageHeader";
import { useNotify } from "../../components/feedback/useNotify";
import { api } from "../../lib/api";
import { qk } from "../../lib/query-keys";
import type { EmployeeSummaryDto } from "../../types/api";

const createSchema = z.object({
  name: z.string().min(2, "Name is required").max(200),
  department: z.string().min(2, "Department is required").max(100),
  position: z.string().min(2, "Position is required").max(100),
  email: z.email("Valid email is required"),
});

const editSchema = z.object({
  name: z.string().min(2, "Name is required").max(200),
  department: z.string().min(2, "Department is required").max(100),
  position: z.string().min(2, "Position is required").max(100),
});

type CreateForm = z.infer<typeof createSchema>;
type EditForm = z.infer<typeof editSchema>;

export function EmployeesPage() {
  const notify = useNotify();
  const queryClient = useQueryClient();

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

  const employeesQuery = useQuery({
    queryKey: qk.employees.list(page, pageSize, department),
    queryFn: () => api.employees.list(page, pageSize, department),
    placeholderData: (prev) => prev,
  });

  const createMutation = useMutation({
    mutationFn: api.employees.create,
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: qk.employees.all });
      setCreateOpen(false);
      createForm.reset();
      notify.success("Employee created successfully");
    },
    onError: notify.error,
  });

  const editMutation = useMutation({
    mutationFn: ({ id, ...payload }: EditForm & { id: string }) =>
      api.employees.update(id, payload),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: qk.employees.all });
      setEditTarget(null);
      notify.success("Employee updated successfully");
    },
    onError: notify.error,
  });

  const deleteMutation = useMutation({
    mutationFn: api.employees.delete,
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: qk.employees.all });
      setDeleteTarget(null);
      notify.success("Employee deleted");
    },
    onError: notify.error,
  });

  const createForm = useForm<CreateForm>({
    resolver: zodResolver(createSchema),
    defaultValues: { name: "", department: "", position: "", email: "" },
  });

  const editForm = useForm<EditForm>({
    resolver: zodResolver(editSchema),
  });

  const openEdit = useCallback(
    (row: EmployeeSummaryDto) => {
      setEditTarget(row);
      editForm.reset({
        name: row.name,
        department: row.department,
        position: row.position,
      });
    },
    [editForm],
  );

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
              <IconButton size="small" onClick={() => openEdit(row)}>
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
    [openEdit],
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

      <Dialog open={createOpen} onClose={() => setCreateOpen(false)} fullWidth maxWidth="sm">
        <DialogTitle>Create Employee</DialogTitle>
        <DialogContent>
          <Stack spacing={2} mt={1}>
            <TextField
              label="Full Name"
              error={!!createForm.formState.errors.name}
              helperText={createForm.formState.errors.name?.message}
              {...createForm.register("name")}
            />
            <TextField
              label="Email"
              type="email"
              error={!!createForm.formState.errors.email}
              helperText={createForm.formState.errors.email?.message}
              {...createForm.register("email")}
            />
            <TextField
              label="Department"
              error={!!createForm.formState.errors.department}
              helperText={createForm.formState.errors.department?.message}
              {...createForm.register("department")}
            />
            <TextField
              label="Position"
              error={!!createForm.formState.errors.position}
              helperText={createForm.formState.errors.position?.message}
              {...createForm.register("position")}
            />
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setCreateOpen(false)}>Cancel</Button>
          <Button
            variant="contained"
            onClick={createForm.handleSubmit((v) => createMutation.mutate(v))}
            disabled={createMutation.isPending}
          >
            {createMutation.isPending ? "Creating..." : "Create"}
          </Button>
        </DialogActions>
      </Dialog>

      <Dialog
        open={!!editTarget}
        onClose={() => setEditTarget(null)}
        fullWidth
        maxWidth="sm"
      >
        <DialogTitle>Edit Employee</DialogTitle>
        <DialogContent>
          <Stack spacing={2} mt={1}>
            <TextField
              label="Full Name"
              error={!!editForm.formState.errors.name}
              helperText={editForm.formState.errors.name?.message}
              {...editForm.register("name")}
            />
            <TextField
              label="Department"
              error={!!editForm.formState.errors.department}
              helperText={editForm.formState.errors.department?.message}
              {...editForm.register("department")}
            />
            <TextField
              label="Position"
              error={!!editForm.formState.errors.position}
              helperText={editForm.formState.errors.position?.message}
              {...editForm.register("position")}
            />
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setEditTarget(null)}>Cancel</Button>
          <Button
            variant="contained"
            onClick={editForm.handleSubmit((v) =>
              editMutation.mutate({ id: editTarget!.id, ...v }),
            )}
            disabled={editMutation.isPending}
          >
            {editMutation.isPending ? "Saving..." : "Save Changes"}
          </Button>
        </DialogActions>
      </Dialog>

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
