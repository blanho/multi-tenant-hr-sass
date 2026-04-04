import { zodResolver } from "@hookform/resolvers/zod";
import AddRoundedIcon from "@mui/icons-material/AddRounded";
import EditRoundedIcon from "@mui/icons-material/EditRounded";
import BlockRoundedIcon from "@mui/icons-material/BlockRounded";
import RestoreRoundedIcon from "@mui/icons-material/RestoreRounded";
import UpgradeRoundedIcon from "@mui/icons-material/UpgradeRounded";
import {
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  IconButton,
  MenuItem,
  Stack,
  TextField,
  Tooltip,
} from "@mui/material";
import { DataGrid, type GridColDef } from "@mui/x-data-grid";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import dayjs from "dayjs";
import { useCallback, useMemo, useState } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { ConfirmDialog } from "../../components/common/ConfirmDialog";
import { EmptyState } from "../../components/common/EmptyState";
import { PageHeader } from "../../components/common/PageHeader";
import { StatusChip } from "../../components/common/StatusChip";
import { useNotify } from "../../components/feedback/useNotify";
import { api } from "../../lib/api";
import { qk } from "../../lib/query-keys";
import type { TenantDto, TenantPlan } from "../../types/api";

const PLANS: TenantPlan[] = ["Free", "Starter", "Professional", "Enterprise"];

const createSchema = z.object({
  name: z.string().min(2).max(200),
  slug: z.string().min(2).max(100).regex(/^[a-z0-9-]+$/, "Lowercase letters, numbers, hyphens only"),
  contactEmail: z.email(),
  plan: z.enum(["Free", "Starter", "Professional", "Enterprise"]),
});

const editSchema = z.object({
  name: z.string().min(2).max(200),
  contactEmail: z.email(),
});

type CreateForm = z.infer<typeof createSchema>;
type EditForm = z.infer<typeof editSchema>;

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
  const [upgradePlan, setUpgradePlan] = useState<TenantPlan>("Starter");

  const tenantsQuery = useQuery({
    queryKey: qk.tenants.all,
    queryFn: api.tenants.list,
  });

  const createMutation = useMutation({
    mutationFn: api.tenants.create,
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: qk.tenants.all });
      setCreateOpen(false);
      createForm.reset();
      notify.success("Tenant created");
    },
    onError: notify.error,
  });

  const editMutation = useMutation({
    mutationFn: ({ id, ...payload }: EditForm & { id: string }) =>
      api.tenants.update(id, payload),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: qk.tenants.all });
      setEditTarget(null);
      notify.success("Tenant updated");
    },
    onError: notify.error,
  });

  const suspendMutation = useMutation({
    mutationFn: (id: string) => api.tenants.suspend(id, "Administrative action"),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: qk.tenants.all });
      setSuspendTarget(null);
      notify.success("Tenant suspended");
    },
    onError: notify.error,
  });

  const reinstateMutation = useMutation({
    mutationFn: api.tenants.reinstate,
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: qk.tenants.all });
      notify.success("Tenant reinstated");
    },
    onError: notify.error,
  });

  const upgradeMutation = useMutation({
    mutationFn: ({ id, plan }: { id: string; plan: string }) =>
      api.tenants.upgradePlan(id, plan),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: qk.tenants.all });
      setUpgradeTarget(null);
      notify.success("Plan upgraded");
    },
    onError: notify.error,
  });

  const createForm = useForm<CreateForm>({
    resolver: zodResolver(createSchema),
    defaultValues: { name: "", slug: "", contactEmail: "", plan: "Free" },
  });

  const editForm = useForm<EditForm>({ resolver: zodResolver(editSchema) });

  const openEdit = useCallback(
    (row: TenantDto) => {
      setEditTarget(row);
      editForm.reset({ name: row.name, contactEmail: row.contactEmail });
    },
    [editForm],
  );

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
              <IconButton size="small" onClick={() => openEdit(row)}>
                <EditRoundedIcon fontSize="small" />
              </IconButton>
            </Tooltip>
            <Tooltip title="Upgrade Plan">
              <IconButton
                size="small"
                color="primary"
                onClick={() => {
                  setUpgradeTarget(row);
                  setUpgradePlan(row.plan === "Free" ? "Starter" : "Enterprise");
                }}
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
    [openEdit, reinstateMutation],
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

      <Dialog open={createOpen} onClose={() => setCreateOpen(false)} fullWidth maxWidth="sm">
        <DialogTitle>Create Tenant</DialogTitle>
        <DialogContent>
          <Stack spacing={2} mt={1}>
            <TextField
              label="Tenant Name"
              error={!!createForm.formState.errors.name}
              helperText={createForm.formState.errors.name?.message}
              {...createForm.register("name")}
            />
            <TextField
              label="Slug"
              error={!!createForm.formState.errors.slug}
              helperText={createForm.formState.errors.slug?.message ?? "URL-friendly identifier (lowercase, hyphens)"}
              {...createForm.register("slug")}
            />
            <TextField
              label="Contact Email"
              type="email"
              error={!!createForm.formState.errors.contactEmail}
              helperText={createForm.formState.errors.contactEmail?.message}
              {...createForm.register("contactEmail")}
            />
            <TextField select label="Plan" defaultValue="Free" {...createForm.register("plan")}>
              {PLANS.map((p) => (
                <MenuItem key={p} value={p}>
                  {p}
                </MenuItem>
              ))}
            </TextField>
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

      <Dialog open={!!editTarget} onClose={() => setEditTarget(null)} fullWidth maxWidth="sm">
        <DialogTitle>Edit Tenant</DialogTitle>
        <DialogContent>
          <Stack spacing={2} mt={1}>
            <TextField
              label="Tenant Name"
              error={!!editForm.formState.errors.name}
              helperText={editForm.formState.errors.name?.message}
              {...editForm.register("name")}
            />
            <TextField
              label="Contact Email"
              type="email"
              error={!!editForm.formState.errors.contactEmail}
              helperText={editForm.formState.errors.contactEmail?.message}
              {...editForm.register("contactEmail")}
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
            Save
          </Button>
        </DialogActions>
      </Dialog>

      <Dialog
        open={!!upgradeTarget}
        onClose={() => setUpgradeTarget(null)}
        fullWidth
        maxWidth="xs"
      >
        <DialogTitle>Upgrade Plan — {upgradeTarget?.name}</DialogTitle>
        <DialogContent>
          <TextField
            select
            fullWidth
            label="New Plan"
            value={upgradePlan}
            onChange={(e) => setUpgradePlan(e.target.value as TenantPlan)}
            sx={{ mt: 1 }}
          >
            {PLANS.filter((p) => p !== "Free").map((p) => (
              <MenuItem key={p} value={p}>
                {p}
              </MenuItem>
            ))}
          </TextField>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setUpgradeTarget(null)}>Cancel</Button>
          <Button
            variant="contained"
            onClick={() => {
              if (upgradeTarget) {
                upgradeMutation.mutate({ id: upgradeTarget.id, plan: upgradePlan });
              }
            }}
            disabled={upgradeMutation.isPending}
          >
            Upgrade
          </Button>
        </DialogActions>
      </Dialog>

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
