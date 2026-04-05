import BadgeRoundedIcon from "@mui/icons-material/BadgeRounded";
import EventNoteRoundedIcon from "@mui/icons-material/EventNoteRounded";
import NotificationsActiveRoundedIcon from "@mui/icons-material/NotificationsActiveRounded";
import PaymentRoundedIcon from "@mui/icons-material/PaymentRounded";
import PeopleRoundedIcon from "@mui/icons-material/PeopleRounded";
import {
  Box,
  Card,
  CardContent,
  Chip,
  Grid,
  LinearProgress,
  Stack,
  Typography,
} from "@mui/material";
import { useQuery } from "@tanstack/react-query";
import dayjs from "dayjs";
import {
  Bar,
  BarChart,
  CartesianGrid,
  Pie,
  PieChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";
import { PageHeader, StatCard, StatusChip } from "@/components";
import { api } from "@/lib/api";
import { qk } from "@/lib/query-keys";
import { PIE_COLORS } from "./constants";
import { seatColor } from "./utils";

export function DashboardPage() {
  const employeesQuery = useQuery({
    queryKey: qk.employees.list(1, 1),
    queryFn: () => api.employees.list(1, 1),
  });

  const pendingLeaveQuery = useQuery({
    queryKey: qk.leave.pending,
    queryFn: api.leave.getPending,
  });

  const notifStatsQuery = useQuery({
    queryKey: qk.notifications.stats,
    queryFn: api.notifications.getStats,
  });

  const subscriptionQuery = useQuery({
    queryKey: qk.billing.subscription,
    queryFn: api.billing.getSubscription,
    retry: 0,
    throwOnError: false,
  });

  const featuresQuery = useQuery({
    queryKey: qk.features.all,
    queryFn: api.features.list,
  });

  const totalEmployees = employeesQuery.data?.totalCount ?? 0;
  const pendingLeaves = pendingLeaveQuery.data?.length ?? 0;
  const notifStats = notifStatsQuery.data;
  const sub = subscriptionQuery.data;
  const seatPercent = sub ? Math.round((sub.usedSeats / sub.maxSeats) * 100) : 0;
  const features = featuresQuery.data ?? [];

  const leaveByType = pendingLeaveQuery.data
    ? Object.entries(
        pendingLeaveQuery.data.reduce<Record<string, number>>((acc, lr) => {
          acc[lr.type] = (acc[lr.type] ?? 0) + 1;
          return acc;
        }, {}),
      ).map(([name, value]) => ({ name, value }))
    : [];

  const notifBarData = notifStats
    ? [
        { name: "Sent", count: notifStats.sentCount },
        { name: "Delivered", count: notifStats.deliveredCount },
        { name: "Read", count: notifStats.readCount },
        { name: "Unread", count: notifStats.unreadCount },
        { name: "Failed", count: notifStats.failedCount },
      ]
    : [];

  const isLoading =
    employeesQuery.isFetching ||
    pendingLeaveQuery.isFetching ||
    notifStatsQuery.isFetching ||
    subscriptionQuery.isFetching;

  return (
    <Stack spacing={2.5}>
      <PageHeader
        title="Dashboard"
        subtitle={`Overview as of ${dayjs().format("MMMM D, YYYY")}`}
      />

      {isLoading && <LinearProgress />}

      <Grid container spacing={2}>
        <Grid size={{ xs: 6, md: 3 }}>
          <StatCard
            label="Total Employees"
            value={totalEmployees}
            icon={<PeopleRoundedIcon color="primary" />}
          />
        </Grid>
        <Grid size={{ xs: 6, md: 3 }}>
          <StatCard
            label="Pending Leave Requests"
            value={pendingLeaves}
            icon={<EventNoteRoundedIcon color="warning" />}
          />
        </Grid>
        <Grid size={{ xs: 6, md: 3 }}>
          <StatCard
            label="Unread Notifications"
            value={notifStats?.unreadCount ?? 0}
            icon={<NotificationsActiveRoundedIcon color="secondary" />}
          />
        </Grid>
        <Grid size={{ xs: 6, md: 3 }}>
          <StatCard
            label="Subscription Plan"
            value={sub?.planName ?? "—"}
            helper={sub ? `${sub.usedSeats}/${sub.maxSeats} seats` : undefined}
            icon={<PaymentRoundedIcon color="success" />}
          />
        </Grid>
      </Grid>

      <Grid container spacing={2}>
        <Grid size={{ xs: 12, md: 6 }}>
          <Card>
            <CardContent>
              <Typography variant="h6" mb={2}>
                Pending Leave by Type
              </Typography>
              {leaveByType.length === 0 ? (
                <Stack alignItems="center" py={4}>
                  <EventNoteRoundedIcon sx={{ fontSize: 40, color: "text.disabled" }} />
                  <Typography variant="body2" color="text.secondary" mt={1}>
                    No pending leave requests
                  </Typography>
                </Stack>
              ) : (
                <ResponsiveContainer width="100%" height={260}>
                  <PieChart>
                    <Pie
                      data={leaveByType.map((entry, i) => ({
                        ...entry,
                        fill: PIE_COLORS[i % PIE_COLORS.length],
                      }))}
                      dataKey="value"
                      nameKey="name"
                      cx="50%"
                      cy="50%"
                      outerRadius={90}
                      label={({ name, value }) => `${name}: ${value}`}
                    />
                    <Tooltip />
                  </PieChart>
                </ResponsiveContainer>
              )}
            </CardContent>
          </Card>
        </Grid>

        <Grid size={{ xs: 12, md: 6 }}>
          <Card>
            <CardContent>
              <Typography variant="h6" mb={2}>
                Notification Overview
              </Typography>
              {notifBarData.length === 0 ? (
                <Stack alignItems="center" py={4}>
                  <NotificationsActiveRoundedIcon
                    sx={{ fontSize: 40, color: "text.disabled" }}
                  />
                  <Typography variant="body2" color="text.secondary" mt={1}>
                    No notification data available
                  </Typography>
                </Stack>
              ) : (
                <ResponsiveContainer width="100%" height={260}>
                  <BarChart data={notifBarData}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="name" />
                    <YAxis allowDecimals={false} />
                    <Tooltip />
                    <Bar dataKey="count" fill="#6366F1" radius={[4, 4, 0, 0]} />
                  </BarChart>
                </ResponsiveContainer>
              )}
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      <Grid container spacing={2}>
        {sub && (
          <Grid key="seat-usage" size={{ xs: 12, md: 6 }}>
            <Card>
              <CardContent>
                <Stack
                  direction="row"
                  justifyContent="space-between"
                  alignItems="center"
                  mb={2}
                >
                  <Typography variant="h6">Seat Usage</Typography>
                  <StatusChip status={sub.status} />
                </Stack>
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
                    color={seatColor(seatPercent)}
                    sx={{ height: 8, borderRadius: 4 }}
                  />
                  {sub.currentPeriodEnd && (
                    <Typography variant="caption" color="text.disabled">
                      Current period ends{" "}
                      {dayjs(sub.currentPeriodEnd).format("MMM D, YYYY")}
                    </Typography>
                  )}
                </Stack>
              </CardContent>
            </Card>
          </Grid>
        )}

        <Grid key="feature-flags" size={{ xs: 12, md: 6 }}>
          <Card>
            <CardContent>
              <Typography variant="h6" mb={2}>
                Feature Flags
              </Typography>
              {features.length === 0 ? (
                <Typography variant="body2" color="text.secondary">
                  No feature flags configured
                </Typography>
              ) : (
                <Box sx={{ display: "flex", flexWrap: "wrap", gap: 1 }}>
                  {features.map((f) => (
                    <Chip
                      key={f.name}
                      icon={<BadgeRoundedIcon />}
                      label={f.name}
                      variant={f.isEnabled ? "filled" : "outlined"}
                      color={f.isEnabled ? "success" : "default"}
                      size="small"
                    />
                  ))}
                </Box>
              )}
            </CardContent>
          </Card>
        </Grid>

        {pendingLeaves > 0 && (
          <Grid key="pending-leave" size={{ xs: 12 }}>
            <Card>
              <CardContent>
                <Typography variant="h6" mb={2}>
                  Recent Pending Leave Requests
                </Typography>
                <Stack spacing={1}>
                  {(pendingLeaveQuery.data ?? []).slice(0, 5).map((lr) => (
                    <Stack
                      key={lr.id}
                      direction="row"
                      justifyContent="space-between"
                      alignItems="center"
                      sx={{
                        py: 1,
                        borderBottom: "1px solid",
                        borderColor: "divider",
                      }}
                    >
                      <Stack direction="row" spacing={2} alignItems="center">
                        <Chip label={lr.type} size="small" variant="outlined" />
                        <Typography variant="body2">
                          {dayjs(lr.startDate).format("MMM D")} —{" "}
                          {dayjs(lr.endDate).format("MMM D, YYYY")}
                        </Typography>
                        <Typography variant="caption" color="text.secondary">
                          ({lr.durationDays} day{lr.durationDays === 1 ? "" : "s"})
                        </Typography>
                      </Stack>
                      <StatusChip status={lr.status} />
                    </Stack>
                  ))}
                </Stack>
              </CardContent>
            </Card>
          </Grid>
        )}
      </Grid>
    </Stack>
  );
}
