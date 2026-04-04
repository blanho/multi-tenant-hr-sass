import {
  Alert,
  Button,
  Card,
  CardContent,
  Grid,
  MenuItem,
  Stack,
  TextField,
} from "@mui/material";
import { DataGrid, type GridColDef } from "@mui/x-data-grid";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useMemo, useState } from "react";
import { PageHeader } from "../../components/common/PageHeader";
import { api } from "../../lib/api";
import { extractErrorMessage } from "../../lib/http";
import type { TenantDto } from "../../types/api";

const plans = ["Free", "Starter", "Professional", "Enterprise"];

export function TenantsPage() {
  const queryClient = useQueryClient();
  const [name, setName] = useState("");
  const [slug, setSlug] = useState("");
  const [contactEmail, setContactEmail] = useState("");
  const [plan, setPlan] = useState("Free");

  const tenantsQuery = useQuery({
    queryKey: ["tenants"],
    queryFn: api.getTenants,
  });

  const createMutation = useMutation({
    mutationFn: api.createTenant,
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ["tenants"] });
      setName("");
      setSlug("");
      setContactEmail("");
      setPlan("Free");
    },
  });

  const suspendMutation = useMutation({
    mutationFn: (id: string) => api.suspendTenant(id, "Administrative action"),
    onSuccess: () => void queryClient.invalidateQueries({ queryKey: ["tenants"] }),
  });

  const reinstateMutation = useMutation({
    mutationFn: api.reinstateTenant,
    onSuccess: () => void queryClient.invalidateQueries({ queryKey: ["tenants"] }),
  });

  const columns = useMemo<GridColDef<TenantDto>[]>(
    () => [
      { field: "name", headerName: "Tenant", flex: 1, minWidth: 160 },
      { field: "slug", headerName: "Slug", flex: 1, minWidth: 120 },
      { field: "plan", headerName: "Plan", width: 140 },
      { field: "status", headerName: "Status", width: 140 },
      { field: "maxEmployees", headerName: "Max Employees", width: 140 },
      {
        field: "actions",
        headerName: "Actions",
        width: 220,
        sortable: false,
        renderCell: ({ row }) => (
          <Stack direction="row" spacing={1}>
            <Button size="small" color="warning" onClick={() => suspendMutation.mutate(row.id)}>
              Suspend
            </Button>
            <Button size="small" color="success" onClick={() => reinstateMutation.mutate(row.id)}>
              Reinstate
            </Button>
          </Stack>
        ),
      },
    ],
    [reinstateMutation, suspendMutation],
  );

  return (
    <Stack spacing={2.5}>
      <PageHeader
        title="Tenant Administration"
        subtitle="Manage tenant lifecycle, plans, and operational state"
      />

      {(tenantsQuery.isError || createMutation.isError) && (
        <Alert severity="error">{extractErrorMessage(tenantsQuery.error ?? createMutation.error)}</Alert>
      )}

      <Card>
        <CardContent>
          <Grid container spacing={2}>
            <Grid size={{ xs: 12, md: 3 }}>
              <TextField fullWidth label="Tenant Name" value={name} onChange={(e) => setName(e.target.value)} />
            </Grid>
            <Grid size={{ xs: 12, md: 3 }}>
              <TextField fullWidth label="Slug" value={slug} onChange={(e) => setSlug(e.target.value)} />
            </Grid>
            <Grid size={{ xs: 12, md: 3 }}>
              <TextField
                fullWidth
                label="Contact Email"
                value={contactEmail}
                onChange={(e) => setContactEmail(e.target.value)}
              />
            </Grid>
            <Grid size={{ xs: 12, md: 2 }}>
              <TextField fullWidth select label="Plan" value={plan} onChange={(e) => setPlan(e.target.value)}>
                {plans.map((item) => (
                  <MenuItem key={item} value={item}>
                    {item}
                  </MenuItem>
                ))}
              </TextField>
            </Grid>
            <Grid size={{ xs: 12, md: 1 }}>
              <Button
                fullWidth
                variant="contained"
                sx={{ height: "100%" }}
                onClick={() => {
                  if (!name || !slug || !contactEmail) return;
                  createMutation.mutate({
                    name,
                    slug,
                    contactEmail,
                    plan: plan as "Free" | "Starter" | "Professional" | "Enterprise",
                  });
                }}
              >
                Add
              </Button>
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      <DataGrid
        autoHeight
        rows={tenantsQuery.data ?? []}
        columns={columns}
        loading={tenantsQuery.isLoading}
        disableRowSelectionOnClick
        pageSizeOptions={[10, 20]}
      />
    </Stack>
  );
}
