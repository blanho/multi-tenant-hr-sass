import { Card, CardContent, Grid, LinearProgress, Stack, Typography } from "@mui/material";
import TrendingUpRoundedIcon from "@mui/icons-material/TrendingUpRounded";
import GroupsRoundedIcon from "@mui/icons-material/GroupsRounded";
import EventBusyRoundedIcon from "@mui/icons-material/EventBusyRounded";
import NotificationsRoundedIcon from "@mui/icons-material/NotificationsRounded";
import { useQuery } from "@tanstack/react-query";
import { Bar, BarChart, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis } from "recharts";
import { PageHeader } from "../../components/common/PageHeader";
import { StatCard } from "../../components/common/StatCard";
import { api } from "../../lib/api";

export function DashboardPage() {
  const employeesQuery = useQuery({
    queryKey: ["employees", 1, 100],
    queryFn: () => api.getEmployees(1, 100),
  });

  const pendingLeavesQuery = useQuery({
    queryKey: ["leave", "pending"],
    queryFn: api.getPendingLeaves,
  });

  const notificationsQuery = useQuery({
    queryKey: ["notifications", "unread-count"],
    queryFn: api.getUnreadCount,
  });

  const subscriptionQuery = useQuery({
    queryKey: ["billing", "subscription"],
    queryFn: api.getSubscription,
    retry: 0,
  });

  const featuresQuery = useQuery({
    queryKey: ["features"],
    queryFn: api.getFeatures,
  });

  const employeeItems = employeesQuery.data?.items ?? [];
  const chartData = Object.values(
    employeeItems.reduce<Record<string, { department: string; count: number }>>((acc, employee) => {
      if (!acc[employee.department]) {
        acc[employee.department] = { department: employee.department, count: 0 };
      }
      acc[employee.department].count += 1;
      return acc;
    }, {}),
  );

  return (
    <Stack spacing={3}>
      <PageHeader
        title="HR Operations Dashboard"
        subtitle="Real-time visibility into employees, leave, notifications, and plan utilization"
      />

      {(employeesQuery.isLoading || pendingLeavesQuery.isLoading || notificationsQuery.isLoading) && (
        <LinearProgress />
      )}

      <Grid container spacing={2}>
        <Grid size={{ xs: 12, md: 3 }}>
          <StatCard
            label="Employees"
            value={employeesQuery.data?.totalCount ?? 0}
            helper="Active records in tenant scope"
            icon={<GroupsRoundedIcon color="primary" />}
          />
        </Grid>
        <Grid size={{ xs: 12, md: 3 }}>
          <StatCard
            label="Pending Leave Requests"
            value={pendingLeavesQuery.data?.length ?? 0}
            helper="Need manager action"
            icon={<EventBusyRoundedIcon color="secondary" />}
          />
        </Grid>
        <Grid size={{ xs: 12, md: 3 }}>
          <StatCard
            label="Unread Notifications"
            value={notificationsQuery.data?.count ?? 0}
            helper="Across channels"
            icon={<NotificationsRoundedIcon color="primary" />}
          />
        </Grid>
        <Grid size={{ xs: 12, md: 3 }}>
          <StatCard
            label="Seat Usage"
            value={
              subscriptionQuery.data
                ? `${subscriptionQuery.data.usedSeats}/${subscriptionQuery.data.maxSeats}`
                : "N/A"
            }
            helper={subscriptionQuery.data?.status ?? "No subscription"}
            icon={<TrendingUpRoundedIcon color="secondary" />}
          />
        </Grid>
      </Grid>

      <Grid container spacing={2}>
        <Grid size={{ xs: 12, md: 8 }}>
          <Card>
            <CardContent>
              <Typography variant="h6" sx={{ mb: 2 }}>
                Headcount by Department
              </Typography>
              <ResponsiveContainer width="100%" height={300}>
                <BarChart data={chartData} margin={{ left: 8, right: 8 }}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="department" />
                  <YAxis />
                  <Tooltip />
                  <Bar dataKey="count" fill="#7C3AED" radius={[8, 8, 0, 0]} />
                </BarChart>
              </ResponsiveContainer>
            </CardContent>
          </Card>
        </Grid>

        <Grid size={{ xs: 12, md: 4 }}>
          <Card>
            <CardContent>
              <Typography variant="h6" sx={{ mb: 2 }}>
                Enabled Feature Flags
              </Typography>
              <Stack spacing={1}>
                {(featuresQuery.data ?? []).slice(0, 8).map((feature) => (
                  <Typography key={feature} variant="body2" color="text.secondary">
                    • {feature}
                  </Typography>
                ))}
              </Stack>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Stack>
  );
}
