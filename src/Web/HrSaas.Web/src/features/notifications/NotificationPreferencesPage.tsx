import SaveRoundedIcon from "@mui/icons-material/SaveRounded";
import TuneRoundedIcon from "@mui/icons-material/TuneRounded";
import {
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  Divider,
  FormControl,
  FormControlLabel,
  Grid,
  InputLabel,
  LinearProgress,
  MenuItem,
  Select,
  Stack,
  Switch,
  TextField,
  Typography,
} from "@mui/material";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { PageHeader } from "../../components/common/PageHeader";
import { useNotify } from "../../components/feedback/useNotify";
import { api } from "../../lib/api";
import { qk } from "../../lib/query-keys";
import type {
  DigestFrequency,
  NotificationCategory,
  NotificationChannel,
  NotificationPreferencesDto,
} from "../../types/api";

const ALL_CHANNELS: NotificationChannel[] = ["Email", "Sms", "InApp", "Push", "Webhook", "Slack"];
const ALL_CATEGORIES: NotificationCategory[] = [
  "System", "Leave", "Employee", "Billing", "Security", "Tenant", "General",
];
const DIGEST_OPTIONS: DigestFrequency[] = ["None", "Daily", "Weekly", "BiWeekly", "Monthly"];

export function NotificationPreferencesPage() {
  const prefsQuery = useQuery({
    queryKey: qk.notifications.preferences,
    queryFn: api.notificationPreferences.get,
  });

  if (prefsQuery.isFetching && !prefsQuery.data) {
    return (
      <Stack spacing={2.5}>
        <PageHeader
          title="Notification Preferences"
          subtitle="Customize how and when you receive notifications"
        />
        <LinearProgress />
      </Stack>
    );
  }

  if (!prefsQuery.data) {
    return (
      <Stack spacing={2.5}>
        <PageHeader
          title="Notification Preferences"
          subtitle="Customize how and when you receive notifications"
        />
        <Typography color="text.secondary">
          Unable to load preferences. Please try again later.
        </Typography>
      </Stack>
    );
  }

  return (
    <PreferencesForm
      key={prefsQuery.dataUpdatedAt}
      initial={prefsQuery.data}
    />
  );
}

function PreferencesForm({ initial }: Readonly<{ initial: NotificationPreferencesDto }>) {
  const notify = useNotify();
  const queryClient = useQueryClient();

  const [enabledChannels, setEnabledChannels] = useState<NotificationChannel[]>(
    initial.enabledChannels,
  );
  const [mutedCategories, setMutedCategories] = useState<NotificationCategory[]>(
    initial.mutedCategories,
  );
  const [emailEnabled, setEmailEnabled] = useState(initial.emailEnabled);
  const [digestFrequency, setDigestFrequency] = useState<DigestFrequency>(
    initial.digestFrequency,
  );
  const [quietStart, setQuietStart] = useState(initial.quietHoursStart ?? "");
  const [quietEnd, setQuietEnd] = useState(initial.quietHoursEnd ?? "");
  const [timezone, setTimezone] = useState(initial.timezone ?? "");

  const updateMutation = useMutation({
    mutationFn: () =>
      api.notificationPreferences.update({
        enabledChannels,
        mutedCategories,
        emailEnabled,
        digestFrequency,
        quietHoursStart: quietStart || undefined,
        quietHoursEnd: quietEnd || undefined,
        timezone: timezone || undefined,
      }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: qk.notifications.preferences });
      notify.success("Preferences saved");
    },
    onError: notify.error,
  });

  const toggleChannel = (ch: NotificationChannel) => {
    setEnabledChannels((prev) =>
      prev.includes(ch) ? prev.filter((c) => c !== ch) : [...prev, ch],
    );
  };

  const toggleMuteCategory = (cat: NotificationCategory) => {
    setMutedCategories((prev) =>
      prev.includes(cat) ? prev.filter((c) => c !== cat) : [...prev, cat],
    );
  };

  return (
    <Stack spacing={2.5}>
      <PageHeader
        title="Notification Preferences"
        subtitle="Customize how and when you receive notifications"
        actions={
          <Button
            variant="contained"
            startIcon={<SaveRoundedIcon />}
            onClick={() => updateMutation.mutate()}
            disabled={updateMutation.isPending}
          >
            {updateMutation.isPending ? "Saving..." : "Save Preferences"}
          </Button>
        }
      />

      <Grid container spacing={2}>
        <Grid size={{ xs: 12, md: 6 }}>
          <Card>
            <CardContent>
              <Stack direction="row" spacing={1} alignItems="center" mb={2}>
                <TuneRoundedIcon color="primary" />
                <Typography variant="h6">Channels</Typography>
              </Stack>
              <Typography variant="body2" color="text.secondary" mb={2}>
                Select which channels you want to receive notifications on
              </Typography>
              <Box sx={{ display: "flex", flexWrap: "wrap", gap: 1 }}>
                {ALL_CHANNELS.map((ch) => (
                  <Chip
                    key={ch}
                    label={ch}
                    clickable
                    color={enabledChannels.includes(ch) ? "primary" : "default"}
                    variant={enabledChannels.includes(ch) ? "filled" : "outlined"}
                    onClick={() => toggleChannel(ch)}
                  />
                ))}
              </Box>

              <Divider sx={{ my: 2 }} />

              <FormControlLabel
                control={
                  <Switch
                    checked={emailEnabled}
                    onChange={(_, v) => setEmailEnabled(v)}
                  />
                }
                label="Email Notifications Enabled"
              />
            </CardContent>
          </Card>
        </Grid>

        <Grid size={{ xs: 12, md: 6 }}>
          <Card>
            <CardContent>
              <Typography variant="h6" mb={2}>
                Muted Categories
              </Typography>
              <Typography variant="body2" color="text.secondary" mb={2}>
                Select categories you want to mute
              </Typography>
              <Box sx={{ display: "flex", flexWrap: "wrap", gap: 1 }}>
                {ALL_CATEGORIES.map((cat) => (
                  <Chip
                    key={cat}
                    label={cat}
                    clickable
                    color={mutedCategories.includes(cat) ? "error" : "default"}
                    variant={mutedCategories.includes(cat) ? "filled" : "outlined"}
                    onClick={() => toggleMuteCategory(cat)}
                  />
                ))}
              </Box>
            </CardContent>
          </Card>
        </Grid>

        <Grid size={{ xs: 12, md: 6 }}>
          <Card>
            <CardContent>
              <Typography variant="h6" mb={2}>
                Digest & Schedule
              </Typography>

              <FormControl fullWidth sx={{ mb: 2 }}>
                <InputLabel>Digest Frequency</InputLabel>
                <Select
                  value={digestFrequency}
                  label="Digest Frequency"
                  onChange={(e) => setDigestFrequency(e.target.value as DigestFrequency)}
                >
                  {DIGEST_OPTIONS.map((d) => (
                    <MenuItem key={d} value={d}>{d}</MenuItem>
                  ))}
                </Select>
              </FormControl>

              <TextField
                fullWidth
                label="Timezone"
                placeholder="America/New_York"
                value={timezone}
                onChange={(e) => setTimezone(e.target.value)}
              />
            </CardContent>
          </Card>
        </Grid>

        <Grid size={{ xs: 12, md: 6 }}>
          <Card>
            <CardContent>
              <Typography variant="h6" mb={2}>
                Quiet Hours
              </Typography>
              <Typography variant="body2" color="text.secondary" mb={2}>
                Notifications will be held during quiet hours
              </Typography>
              <Stack direction="row" spacing={2}>
                <TextField
                  fullWidth
                  label="Start Time"
                  type="time"
                  slotProps={{ inputLabel: { shrink: true } }}
                  value={quietStart}
                  onChange={(e) => setQuietStart(e.target.value)}
                />
                <TextField
                  fullWidth
                  label="End Time"
                  type="time"
                  slotProps={{ inputLabel: { shrink: true } }}
                  value={quietEnd}
                  onChange={(e) => setQuietEnd(e.target.value)}
                />
              </Stack>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Stack>
  );
}
