import AddRoundedIcon from "@mui/icons-material/AddRounded";
import BlockRoundedIcon from "@mui/icons-material/BlockRounded";
import EditRoundedIcon from "@mui/icons-material/EditRounded";
import RestoreRoundedIcon from "@mui/icons-material/RestoreRounded";
import UpgradeRoundedIcon from "@mui/icons-material/UpgradeRounded";
import { IconButton, Stack, Tooltip } from "@mui/material";
import { Button } from "@mui/material";
import { DataGrid, type GridColDef } from "@mui/x-data-grid";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import dayjs from "dayjs";
import { useMemo, useState } from "react";
import { ConfirmDialog, EmptyState, PageHeader, StatusChip } from "@/components";
import { useNotify } from "@/hooks/useNotify";
import { qk } from "@/lib/query-keys";
import { tenantsApi } from "./api";
import type { TenantDto } from "./types";
import type { TenantPlan } from "@/types/shared";
import type { CreateTenantForm, EditTenantForm } from "./schemas";
import { CreateTenantDialog } from "./CreateTenantDialog";
import { EditTenantDialog } from "./EditTenantDialog";
import { UpgradePlanDialog } from "./UpgradePlanDialog";

function TenantsEmpty() {
  return (
    <EmptyState
      title="No tenants"
      description="Create your first tenant to get started"
    />
  );
}

export function TenantsPage() {
  const notify = useNotify();
  const queryClient = useQueryClient();

  const [createOpen, setCreateOpen] = useState(false);
  const [editTarget, setEditTarget] = useState<TenantDto | null>(null);
  const [suspendTarget, setSuspendTarget] = useState<TenantDto | null>(null);
  const [upgradeTarget, setUpgradeTarget] = useState<TenantDto | null>(null);

  const tenantsQuery = useQuery({
    queryKey: qk.tenants.all,
    queryFn: tenantsApi.list,
  });

  const createMutation = useMutation({
    mutationFn: (data: CreateTenantForm) => tenantsApi.create(data),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: qk.tenants.all });
      setCreateOpen(false);
      notify.success("Tenant created");
    },
    onError: notify.error,
  });

  const editMutation = useMutation({
    mutationFn: ({ id, ...payload }: EditTenantForm & { id: string }) =>
      tenantsApi.update(id, payload),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: qk.tenants.all });
      setEditTarget(null);
      notify.success("Tenant updated");
    },
    onError: notify.error,
  });

  const suspendMutation = useMutation({
    mutationFn: (id: string) => tenantsApi.suspend(id, "Administrative action"),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: qk.tenants.all });
      setSuspendTarget(null);
      notify.success("Tenant suspended");
    },
    onError: notify.error,
  });

  const reinstateMutation = useMutation({
    mutationFn: tenantsApi.reinstate,
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: qk.tenants.all });
      notify.success("Tenant reinstated");
    },
    onError: notify.error,
  });

  const upgradeMutation = useMutation({
    mutationFn: ({ id, plan }: { id: string; plan: string }) =>
      tenantsApi.upgradePlan(id, plan),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: qk.tenants.all });
      setUpgradeTarget(null);
      notify.success("Plan upgraded");
    },
    onError: notify.error,
  });

  const columns = useMemo<GridColDef<TenantDto>[]>(
    () => [
      { field: "name", headerName: "Tenant Name", flex: 1, minWidth: 160 },
      { field: "slug", headerName: "Slug", width: 140 },
      { field: "contactEmail", headerName: "Contact", flex: 1, minWidth: 180 },
      {
        field: "plan",
        headerName: "Plan",
        width: 130,
        renderCell: ({ value }) => <StatusChip status={String(value)} />,
      },
      {
        field: "status",
        headerName: "Status",
        width: 130,
        renderCell: ({ value }) => <StatusChip status={String(value)} />,
      },
      { field: "maxEmployees", headerName: "Max Emp.", width: 100, align: "center" },
      {
        field: "createdAt",
        headerName: "Created",
        width: 120,
        valueFormatter: (value: string) => dayjs(value).format("MMM D, YYYY"),
      },
      {
        field: "actions",
        headerName: "Actions",
        width: 180,
        sortable: false,
        renderCell: ({ row }) => (
          <Stack direction="row" spacing={0.5}>
            <Tooltip title="Edit">
              <IconButton size="small" onClick={() => setEditTarget(row)}>
                <EditRoundedIcon fontSize="small" />
              </IconButton>
            </Tooltip>
            <Tooltip title="Upgrade Plan">
              <IconButton
                size="small"
                color="primary"
                onClick={() => setUpgradeTarget(row)}
              >
                <UpgradeRoundedIcon fontSize="small" />
              </IconButton>
            </Tooltip>
            {row.status === "Active" ? (
              <Tooltip title="Suspend">
                <IconButton
                  size="small"
                  color="error"
                  onClick={() => setSuspendTarget(row)}
                >
                  <BlockRoundedIcon fontSize="small" />
                </IconButton>
              </Tooltip>
            ) : (
              <Tooltip title="Reinstate">
                <IconButton
                  size="small"
                  color="success"
                  onClick={() => reinstateMutation.mutate(row.id)}
                >
                  <RestoreRoundedIcon fontSize="small" />
                </IconButton>
              </Tooltip>
            )}
          </Stack>
        ),
      },
    ],
    [reinstateMutation],
  );

  return (
    <Stack spacing={2.5}>
      <PageHeader
        title="Tenant Administration"
        subtitle="Manage tenant lifecycle, plans, and operational state"
        actions={
          <Button
            startIcon={<AddRoundedIcon />}
            variant="contained"
            onClick={() => setCreateOpen(true)}
          >
            Create Tenant
          </Button>
        }
      />

      <DataGrid
        autoHeight
        rows={tenantsQuery.data ?? []}
        columns={columns}
        loading={tenantsQuery.isLoading}
        disableRowSelectionOnClick
        pageSizeOptions={[10, 20, 50]}
        initialState={{ pagination: { paginationModel: { pageSize: 10, page: 0 } } }}
        slots={{ noRowsOverlay: TenantsEmpty }}
        sx={{ minHeight: 400 }}
      />

      <CreateTenantDialog
        open={createOpen}
        onClose={() => setCreateOpen(false)}
        onSubmit={(data) => createMutation.mutate(data)}
        loading={createMutation.isPending}
      />

      <EditTenantDialog
        open={!!editTarget}
        tenant={editTarget}
        onClose={() => setEditTarget(null)}
        onSubmit={(data) => editMutation.mutate(data)}
        loading={editMutation.isPending}
      />

      <UpgradePlanDialog
        open={!!upgradeTarget}
        tenant={upgradeTarget}
        onClose={() => setUpgradeTarget(null)}
        onSubmit={(id: string, plan: TenantPlan) =>
          upgradeMutation.mutate({ id, plan })
        }
        loading={upgradeMutation.isPending}
      />

      <ConfirmDialog
        open={!!suspendTarget}
        title="Suspend Tenant"
        message={`Suspending "${suspendTarget?.name}" will disable all access for their users. Continue?`}
        confirmLabel="Suspend"
        severity="error"
        loading={suspendMutation.isPending}
        onConfirm={() => suspendTarget && suspendMutation.mutate(suspendTarget.id)}
        onCancel={() => setSuspendTarget(null)}
      />
    </Stack>
  );
}
