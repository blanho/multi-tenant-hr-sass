import CancelRoundedIcon from "@mui/icons-material/CancelRounded";
import CreditScoreRoundedIcon from "@mui/icons-material/CreditScoreRounded";
import RocketLaunchRoundedIcon from "@mui/icons-material/RocketLaunchRounded";
import {
  Button,
  Card,
  CardContent,
  Divider,
  Grid,
  LinearProgress,
  Stack,
  Typography,
} from "@mui/material";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import dayjs from "dayjs";
import { useState } from "react";
import { DetailRow, EmptyState, PageHeader, StatusChip } from "@/components";
import { useNotify } from "@/hooks/useNotify";
import { qk } from "@/lib/query-keys";
import { billingApi } from "./api";
import type { BillingCycle } from "@/types/shared";
import { ActivateSubscriptionDialog } from "./ActivateSubscriptionDialog";
import { CancelSubscriptionDialog } from "./CancelSubscriptionDialog";

export function BillingPage() {
  const notify = useNotify();
  const queryClient = useQueryClient();
  const [cancelOpen, setCancelOpen] = useState(false);
  const [activateOpen, setActivateOpen] = useState(false);

  const subscriptionQuery = useQuery({
    queryKey: qk.billing.subscription,
    queryFn: billingApi.getSubscription,
    retry: 0,
  });

  const createFreeMutation = useMutation({
    mutationFn: billingApi.createFree,
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: qk.billing.subscription });
      notify.success("Free subscription activated");
    },
    onError: notify.error,
  });

  const activateMutation = useMutation({
    mutationFn: ({
      price,
      cycle,
      externalId,
    }: {
      price: number;
      cycle: BillingCycle;
      externalId?: string;
    }) => {
      const sub = subscriptionQuery.data;
      if (!sub) return Promise.reject(new Error("No subscription"));
      return billingApi.activate(sub.id, { price, cycle, externalId });
    },
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: qk.billing.subscription });
      setActivateOpen(false);
      notify.success("Subscription activated");
    },
    onError: notify.error,
  });

  const cancelMutation = useMutation({
    mutationFn: (reason: string) => {
      const sub = subscriptionQuery.data;
      if (!sub) return Promise.reject(new Error("No subscription"));
      return billingApi.cancel({ subscriptionId: sub.id, reason });
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
                <Stack
                  direction="row"
                  justifyContent="space-between"
                  alignItems="center"
                  mb={2}
                >
                  <Typography variant="h6">Subscription Details</Typography>
                  <StatusChip status={sub.status} />
                </Stack>
                <Divider />
                <Stack mt={1}>
                  <DetailRow label="Plan" value={sub.planName} />
                  <DetailRow label="Billing Cycle" value={sub.billingCycle} />
                  <DetailRow label="Price" value={`$${sub.pricePerCycle.toFixed(2)}`} />
                  <DetailRow
                    label="Created"
                    value={dayjs(sub.createdAt).format("MMM D, YYYY")}
                  />
                  {sub.currentPeriodEnd && (
                    <DetailRow
                      label="Current Period Ends"
                      value={dayjs(sub.currentPeriodEnd).format("MMM D, YYYY")}
                    />
                  )}
                  {sub.trialEndsAt && (
                    <DetailRow
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

      <ActivateSubscriptionDialog
        open={activateOpen}
        onClose={() => setActivateOpen(false)}
        onSubmit={(price, cycle, externalId) =>
          activateMutation.mutate({ price, cycle, externalId })
        }
        loading={activateMutation.isPending}
      />

      <CancelSubscriptionDialog
        open={cancelOpen}
        onClose={() => setCancelOpen(false)}
        onConfirm={(reason) => cancelMutation.mutate(reason)}
        loading={cancelMutation.isPending}
      />
    </Stack>
  );
}
