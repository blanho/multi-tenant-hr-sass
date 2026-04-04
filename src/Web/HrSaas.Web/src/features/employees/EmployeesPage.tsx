import { zodResolver } from "@hookform/resolvers/zod";
import AddRoundedIcon from "@mui/icons-material/AddRounded";
import DeleteOutlineRoundedIcon from "@mui/icons-material/DeleteOutlineRounded";
import {
  Alert,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Stack,
  TextField,
} from "@mui/material";
import { DataGrid, type GridColDef } from "@mui/x-data-grid";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useMemo, useState } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { PageHeader } from "../../components/common/PageHeader";
import { api } from "../../lib/api";
import { extractErrorMessage } from "../../lib/http";
import type { EmployeeSummaryDto } from "../../types/api";

const schema = z.object({
  name: z.string().min(2),
  department: z.string().min(2),
  position: z.string().min(2),
  email: z.email(),
});

type EmployeeForm = z.infer<typeof schema>;

export function EmployeesPage() {
  const [open, setOpen] = useState(false);
  const queryClient = useQueryClient();

  const employeesQuery = useQuery({
    queryKey: ["employees", 1, 100],
    queryFn: () => api.getEmployees(1, 100),
  });

  const createMutation = useMutation({
    mutationFn: api.createEmployee,
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ["employees"] });
      setOpen(false);
      reset();
    },
  });

  const deleteMutation = useMutation({
    mutationFn: api.deleteEmployee,
    onSuccess: () => void queryClient.invalidateQueries({ queryKey: ["employees"] }),
  });

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<EmployeeForm>({ resolver: zodResolver(schema) });

  const columns = useMemo<GridColDef<EmployeeSummaryDto>[]>(
    () => [
      { field: "name", headerName: "Name", flex: 1, minWidth: 180 },
      { field: "department", headerName: "Department", flex: 1, minWidth: 150 },
      { field: "position", headerName: "Position", flex: 1, minWidth: 150 },
      {
        field: "actions",
        headerName: "Actions",
        width: 120,
        sortable: false,
        renderCell: ({ row }) => (
          <Button
            color="error"
            size="small"
            startIcon={<DeleteOutlineRoundedIcon />}
            onClick={() => deleteMutation.mutate(row.id)}
          >
            Delete
          </Button>
        ),
      },
    ],
    [deleteMutation],
  );

  return (
    <Stack spacing={2.5}>
      <PageHeader
        title="Employees"
        subtitle="Tenant-isolated employee directory with fast search and CRUD operations"
        actions={
          <Button startIcon={<AddRoundedIcon />} variant="contained" onClick={() => setOpen(true)}>
            Add Employee
          </Button>
        }
      />

      {(employeesQuery.isError || deleteMutation.isError || createMutation.isError) && (
        <Alert severity="error">
          {extractErrorMessage(
            employeesQuery.error ?? deleteMutation.error ?? createMutation.error,
          )}
        </Alert>
      )}

      <DataGrid
        autoHeight
        rows={employeesQuery.data?.items ?? []}
        columns={columns}
        loading={employeesQuery.isLoading}
        disableRowSelectionOnClick
        pageSizeOptions={[10, 20, 50]}
        initialState={{ pagination: { paginationModel: { pageSize: 10, page: 0 } } }}
      />

      <Dialog open={open} onClose={() => setOpen(false)} fullWidth maxWidth="sm">
        <DialogTitle>Create Employee</DialogTitle>
        <DialogContent>
          <Stack spacing={2} mt={1}>
            <TextField label="Full Name" {...register("name")} error={!!errors.name} />
            <TextField label="Email" {...register("email")} error={!!errors.email} />
            <TextField label="Department" {...register("department")} error={!!errors.department} />
            <TextField label="Position" {...register("position")} error={!!errors.position} />
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpen(false)}>Cancel</Button>
          <Button
            variant="contained"
            onClick={handleSubmit((values) => createMutation.mutate(values))}
            disabled={createMutation.isPending}
          >
            Save
          </Button>
        </DialogActions>
      </Dialog>
    </Stack>
  );
}
