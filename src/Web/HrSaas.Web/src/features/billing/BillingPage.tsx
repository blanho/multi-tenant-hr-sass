import CancelRoundedIcon from "@mui/icons-material/CancelRounded";
import CreditScoreRoundedIcon from "@mui/icons-material/CreditScoreRounded";
import RocketLaunchRoundedIcon from "@mui/icons-material/RocketLaunchRounded";
import {
  Button,
  Card,
  CardContent,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Divider,
  FormControl,
  Grid,
  InputAdornment,
  InputLabel,
  LinearProgress,
  MenuItem,
  Select,
  Stack,
  TextField,
  Typography,
} from "@mui/material";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import dayjs from "dayjs";
import { useState } from "react";
import { EmptyState } from "../../components/common/EmptyState";
import { PageHeader } from "../../components/common/PageHeader";
import { StatusChip } from "../../components/common/StatusChip";
import { useNotify } from "../../components/feedback/useNotify";
import { api } from "../../lib/api";
import { qk } from "../../lib/query-keys";
import type { BillingCycle } from "../../types/api";

function InfoRow({ label, value }: Readonly<{ label: string; value: string | number }>) {
  return (
    <Stack direction="row" justifyContent="space-between" py={0.75}>
      <Typography variant="body2" color="text.secondary">
        {label}
      </Typography>
      <Typography variant="body2" fontWeight={600}>
        {value}
      </Typography>
    </Stack>
  );
}

const BILLING_CYCLES: BillingCycle[] = ["Monthly", "Annual"];

export function BillingPage() {
  const notify = useNotify();
  const queryClient = useQueryClient();
  const [cancelOpen, setCancelOpen] = useState(false);
  const [cancelReason, setCancelReason] = useState("Cost optimization");
  const [activateOpen, setActivateOpen] = useState(false);
  const [activatePrice, setActivatePrice] = useState("29.99");
  const [activateCycle, setActivateCycle] = useState<BillingCycle>("Monthly");
  const [activateExternalId, setActivateExternalId] = useState("");

  const subscriptionQuery = useQuery({
    queryKey: qk.billing.subscription,
    queryFn: api.billing.getSubscription,
    retry: 0,
  });

  const createFreeMutation = useMutation({
    mutationFn: api.billing.createFree,
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: qk.billing.subscription });
      notify.success("Free subscription activated");
    },
    onError: notify.error,
  });

  const activateMutation = useMutation({
    mutationFn: () => {
      const sub = subscriptionQuery.data;
      if (!sub) return Promise.reject(new Error("No subscription"));
      return api.billing.activate(sub.id, {
        price: Number.parseFloat(activatePrice),
        cycle: activateCycle,
        externalId: activateExternalId || undefined,
      });
    },
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: qk.billing.subscription });
      setActivateOpen(false);
      notify.success("Subscription activated");
    },
    onError: notify.error,
  });

  const cancelMutation = useMutation({
    mutationFn: () => {
      const sub = subscriptionQuery.data;
      if (!sub) return Promise.reject(new Error("No subscription"));
      return api.billing.cancel({ subscriptionId: sub.id, reason: cancelReason });
    },
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: qk.billing.subscription });
      setCancelOpen(false);
      notify.success("Subscription cancelled");
    },
    onError: notify.error,
  });

  const sub = subscriptionQuery.data;
  const seatPercent = sub ? Math.round((sub.usedSeats / sub.maxSeats) * 100) : 0;

  function seatColor(): "error" | "warning" | "primary" {
    if (seatPercent > 85) return "error";
    if (seatPercent > 60) return "warning";
    return "primary";
  }

  return (
    <Stack spacing={2.5}>
      <PageHeader
        title="Billing & Subscription"
        subtitle="Manage your subscription plan, seat utilization, and billing cycle"
      />

      {subscriptionQuery.isFetching && <LinearProgress />}

      {!sub && !subscriptionQuery.isFetching && (
        <Card>
          <CardContent>
            <EmptyState
              title="No Active Subscription"
              description="Get started by activating a free subscription plan"
              action={
                <Button
                  variant="contained"
                  startIcon={<CreditScoreRoundedIcon />}
                  onClick={() => createFreeMutation.mutate()}
                  disabled={createFreeMutation.isPending}
                >
                  {createFreeMutation.isPending ? "Activating..." : "Activate Free Plan"}
                </Button>
              }
            />
          </CardContent>
        </Card>
      )}

      {sub && (
        <Grid container spacing={2}>
          <Grid size={{ xs: 12, md: 6 }}>
            <Card>
              <CardContent>
                <Stack direction="row" justifyContent="space-between" alignItems="center" mb={2}>
                  <Typography variant="h6">Subscription Details</Typography>
                  <StatusChip status={sub.status} />
                </Stack>
                <Divider />
                <Stack mt={1}>
                  <InfoRow label="Plan" value={sub.planName} />
                  <InfoRow label="Billing Cycle" value={sub.billingCycle} />
                  <InfoRow label="Price" value={`$${sub.pricePerCycle.toFixed(2)}`} />
                  <InfoRow label="Created" value={dayjs(sub.createdAt).format("MMM D, YYYY")} />
                  {sub.currentPeriodEnd && (
                    <InfoRow
                      label="Current Period Ends"
                      value={dayjs(sub.currentPeriodEnd).format("MMM D, YYYY")}
                    />
                  )}
                  {sub.trialEndsAt && (
                    <InfoRow
                      label="Trial Ends"
                      value={dayjs(sub.trialEndsAt).format("MMM D, YYYY")}
                    />
                  )}
                </Stack>
              </CardContent>
            </Card>
          </Grid>

          <Grid size={{ xs: 12, md: 6 }}>
            <Card>
              <CardContent>
                <Typography variant="h6" mb={2}>
                  Seat Utilization
                </Typography>
                <Stack spacing={1}>
                  <Stack direction="row" justifyContent="space-between">
                    <Typography variant="body2" color="text.secondary">
                      {sub.usedSeats} of {sub.maxSeats} seats used
                    </Typography>
                    <Typography variant="body2" fontWeight={600}>
                      {seatPercent}%
                    </Typography>
                  </Stack>
                  <LinearProgress
                      variant="determinate"
                      value={seatPercent}
                      color={seatColor()}
                      sx={{ height: 8, borderRadius: 4 }}
                    />
                  <Typography variant="caption" color="text.disabled">
                    {sub.maxSeats - sub.usedSeats} seats remaining
                  </Typography>
                </Stack>

                <Divider sx={{ my: 2 }} />

                <Stack direction="row" spacing={2}>
                  <Button
                    variant="contained"
                    startIcon={<RocketLaunchRoundedIcon />}
                    onClick={() => setActivateOpen(true)}
                  >
                    Activate / Upgrade
                  </Button>
                  <Button
                    color="error"
                    variant="outlined"
                    startIcon={<CancelRoundedIcon />}
                    onClick={() => setCancelOpen(true)}
                  >
                    Cancel
                  </Button>
                </Stack>
              </CardContent>
            </Card>
          </Grid>
        </Grid>
      )}

      <Dialog open={activateOpen} onClose={() => setActivateOpen(false)} fullWidth maxWidth="xs">
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
              value={activatePrice}
              onChange={(e) => setActivatePrice(e.target.value)}
              slotProps={{
                input: {
                  startAdornment: <InputAdornment position="start">$</InputAdornment>,
                },
              }}
            />
            <FormControl fullWidth>
              <InputLabel>Billing Cycle</InputLabel>
              <Select
                value={activateCycle}
                label="Billing Cycle"
                onChange={(e) => setActivateCycle(e.target.value as BillingCycle)}
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
              value={activateExternalId}
              onChange={(e) => setActivateExternalId(e.target.value)}
            />
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setActivateOpen(false)}>Cancel</Button>
          <Button
            variant="contained"
            onClick={() => activateMutation.mutate()}
            disabled={activateMutation.isPending || !activatePrice}
          >
            {activateMutation.isPending ? "Activating..." : "Activate"}
          </Button>
        </DialogActions>
      </Dialog>

      <Dialog open={cancelOpen} onClose={() => setCancelOpen(false)} fullWidth maxWidth="xs">
        <DialogTitle>Cancel Subscription</DialogTitle>
        <DialogContent>
          <Typography variant="body2" color="text.secondary" mb={2}>
            Please tell us why you are cancelling. This helps us improve.
          </Typography>
          <TextField
            fullWidth
            multiline
            minRows={2}
            label="Reason"
            value={cancelReason}
            onChange={(e) => setCancelReason(e.target.value)}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setCancelOpen(false)}>Keep Subscription</Button>
          <Button
            variant="contained"
            color="error"
            onClick={() => cancelMutation.mutate()}
            disabled={cancelMutation.isPending || !cancelReason}
          >
            {cancelMutation.isPending ? "Cancelling..." : "Confirm Cancel"}
          </Button>
        </DialogActions>
      </Dialog>
    </Stack>
  );
}
