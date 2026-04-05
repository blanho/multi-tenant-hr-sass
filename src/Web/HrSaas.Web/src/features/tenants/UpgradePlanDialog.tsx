import {
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  MenuItem,
  TextField,
} from "@mui/material";
import { useState } from "react";
import type { TenantPlan } from "@/types/shared";
import type { TenantDto } from "./types";
import { PLANS } from "./constants";

interface UpgradePlanDialogProps {
  open: boolean;
  tenant: TenantDto | null;
  onClose: () => void;
  onSubmit: (id: string, plan: TenantPlan) => void;
  loading: boolean;
}

export function UpgradePlanDialog({
  open,
  tenant,
  onClose,
  onSubmit,
  loading,
}: Readonly<UpgradePlanDialogProps>) {
  const [plan, setPlan] = useState<TenantPlan>("Starter");

  return (
    <Dialog open={open} onClose={onClose} fullWidth maxWidth="xs">
      <DialogTitle>Upgrade Plan — {tenant?.name}</DialogTitle>
      <DialogContent>
        <TextField
          select
          fullWidth
          label="New Plan"
          value={plan}
          onChange={(e) => setPlan(e.target.value as TenantPlan)}
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
        <Button onClick={onClose}>Cancel</Button>
        <Button
          variant="contained"
          onClick={() => tenant && onSubmit(tenant.id, plan)}
          disabled={loading}
        >
          Upgrade
        </Button>
      </DialogActions>
    </Dialog>
  );
}
