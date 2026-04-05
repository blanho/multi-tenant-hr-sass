import {
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControl,
  InputAdornment,
  InputLabel,
  MenuItem,
  Select,
  Stack,
  TextField,
  Typography,
} from "@mui/material";
import { useState } from "react";
import type { BillingCycle } from "@/types/shared";
import { BILLING_CYCLES } from "./constants";

interface ActivateSubscriptionDialogProps {
  open: boolean;
  onClose: () => void;
  onSubmit: (price: number, cycle: BillingCycle, externalId?: string) => void;
  loading: boolean;
}

export function ActivateSubscriptionDialog({
  open,
  onClose,
  onSubmit,
  loading,
}: Readonly<ActivateSubscriptionDialogProps>) {
  const [price, setPrice] = useState("29.99");
  const [cycle, setCycle] = useState<BillingCycle>("Monthly");
  const [externalId, setExternalId] = useState("");

  return (
    <Dialog open={open} onClose={onClose} fullWidth maxWidth="xs">
      <DialogTitle>Activate Subscription</DialogTitle>
      <DialogContent>
        <Stack spacing={2} mt={1}>
          <Typography variant="body2" color="text.secondary">
            Set the pricing and billing cycle for this subscription
          </Typography>
          <TextField
            fullWidth
            label="Price per Cycle"
            type="number"
            value={price}
            onChange={(e) => setPrice(e.target.value)}
            slotProps={{
              input: {
                startAdornment: <InputAdornment position="start">$</InputAdornment>,
              },
            }}
          />
          <FormControl fullWidth>
            <InputLabel>Billing Cycle</InputLabel>
            <Select
              value={cycle}
              label="Billing Cycle"
              onChange={(e) => setCycle(e.target.value as BillingCycle)}
            >
              {BILLING_CYCLES.map((c) => (
                <MenuItem key={c} value={c}>
                  {c}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
          <TextField
            fullWidth
            label="External ID (optional)"
            placeholder="Stripe subscription ID"
            value={externalId}
            onChange={(e) => setExternalId(e.target.value)}
          />
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
        <Button
          variant="contained"
          onClick={() =>
            onSubmit(Number.parseFloat(price), cycle, externalId || undefined)
          }
          disabled={loading || !price}
        >
          {loading ? "Activating..." : "Activate"}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
