import {
  Alert,
  Button,
  Card,
  CardContent,
  Chip,
  Grid,
  Stack,
  TextField,
  Typography,
} from "@mui/material";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { PageHeader } from "../../components/common/PageHeader";
import { api } from "../../lib/api";
import { extractErrorMessage } from "../../lib/http";

export function BillingPage() {
  const [reason, setReason] = useState("Cost optimization");
  const queryClient = useQueryClient();

  const subscriptionQuery = useQuery({
    queryKey: ["billing", "subscription"],
    queryFn: api.getSubscription,
    retry: 0,
  });

  const createFreeMutation = useMutation({
    mutationFn: api.createFreeSubscription,
    onSuccess: () => void queryClient.invalidateQueries({ queryKey: ["billing"] }),
  });

  const cancelMutation = useMutation({
    mutationFn: () =>
      subscriptionQuery.data
        ? api.cancelSubscription(subscriptionQuery.data.id, reason)
        : Promise.reject(new Error("No subscription found")),
    onSuccess: () => void queryClient.invalidateQueries({ queryKey: ["billing"] }),
  });

  return (
    <Stack spacing={2.5}>
      <PageHeader
        title="Billing"
        subtitle="Track subscription lifecycle, seat utilization, and plan status"
      />

      {(subscriptionQuery.isError || createFreeMutation.isError || cancelMutation.isError) && (
        <Alert severity="error">
          {extractErrorMessage(
            subscriptionQuery.error ?? createFreeMutation.error ?? cancelMutation.error,
          )}
        </Alert>
      )}

      <Card>
        <CardContent>
          {subscriptionQuery.data ? (
            <Grid container spacing={2}>
              <Grid size={{ xs: 12, md: 3 }}>
                <Typography variant="body2" color="text.secondary">
                  Plan
                </Typography>
                <Typography variant="h6">{subscriptionQuery.data.planName}</Typography>
              </Grid>
              <Grid size={{ xs: 12, md: 2 }}>
                <Typography variant="body2" color="text.secondary">
                  Status
                </Typography>
                <Chip size="small" label={subscriptionQuery.data.status} color="primary" />
              </Grid>
              <Grid size={{ xs: 12, md: 2 }}>
                <Typography variant="body2" color="text.secondary">
                  Billing Cycle
                </Typography>
                <Typography variant="h6">{subscriptionQuery.data.billingCycle}</Typography>
              </Grid>
              <Grid size={{ xs: 12, md: 2 }}>
                <Typography variant="body2" color="text.secondary">
                  Price
                </Typography>
                <Typography variant="h6">${subscriptionQuery.data.pricePerCycle}</Typography>
              </Grid>
              <Grid size={{ xs: 12, md: 3 }}>
                <Typography variant="body2" color="text.secondary">
                  Seats
                </Typography>
                <Typography variant="h6">
                  {subscriptionQuery.data.usedSeats}/{subscriptionQuery.data.maxSeats}
                </Typography>
              </Grid>
            </Grid>
          ) : (
            <Typography color="text.secondary">No active subscription.</Typography>
          )}
        </CardContent>
      </Card>

      <Card>
        <CardContent>
          <Stack direction={{ xs: "column", md: "row" }} spacing={2} alignItems={{ xs: "stretch", md: "center" }}>
            <Button variant="outlined" onClick={() => createFreeMutation.mutate()}>
              Create Free Subscription
            </Button>

            <TextField
              label="Cancel reason"
              value={reason}
              onChange={(e) => setReason(e.target.value)}
              sx={{ minWidth: 280 }}
            />
            <Button color="error" variant="contained" onClick={() => cancelMutation.mutate()}>
              Cancel Subscription
            </Button>
          </Stack>
        </CardContent>
      </Card>
    </Stack>
  );
}
