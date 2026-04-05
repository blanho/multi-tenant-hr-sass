import {
  Collapse,
  FormControl,
  FormControlLabel,
  InputLabel,
  MenuItem,
  Select,
  Stack,
  Switch,
} from "@mui/material";
import type { NotificationCategory, NotificationChannel } from "@/types/api";

const CHANNELS: NotificationChannel[] = ["Email", "Sms", "InApp", "Push", "Webhook", "Slack"];
const CATEGORIES: NotificationCategory[] = [
  "System", "Leave", "Employee", "Billing", "Security", "Tenant", "General",
];

interface NotificationFiltersProps {
  open: boolean;
  channel: NotificationChannel | "";
  category: NotificationCategory | "";
  unreadOnly: boolean;
  onChannelChange: (value: NotificationChannel | "") => void;
  onCategoryChange: (value: NotificationCategory | "") => void;
  onUnreadOnlyChange: (value: boolean) => void;
}

export function NotificationFilters({
  open,
  channel,
  category,
  unreadOnly,
  onChannelChange,
  onCategoryChange,
  onUnreadOnlyChange,
}: Readonly<NotificationFiltersProps>) {
  return (
    <Collapse in={open}>
      <Stack direction="row" spacing={2} mb={2} alignItems="center">
        <FormControl size="small" sx={{ minWidth: 130 }}>
          <InputLabel>Channel</InputLabel>
          <Select
            value={channel}
            label="Channel"
            onChange={(e) => onChannelChange(e.target.value)}
          >
            <MenuItem value="">All</MenuItem>
            {CHANNELS.map((c) => (
              <MenuItem key={c} value={c}>{c}</MenuItem>
            ))}
          </Select>
        </FormControl>

        <FormControl size="small" sx={{ minWidth: 150 }}>
          <InputLabel>Category</InputLabel>
          <Select
            value={category}
            label="Category"
            onChange={(e) => onCategoryChange(e.target.value)}
          >
            <MenuItem value="">All</MenuItem>
            {CATEGORIES.map((c) => (
              <MenuItem key={c} value={c}>{c}</MenuItem>
            ))}
          </Select>
        </FormControl>

        <FormControlLabel
          control={
            <Switch
              size="small"
              checked={unreadOnly}
              onChange={(_, v) => onUnreadOnlyChange(v)}
            />
          }
          label="Unread Only"
        />
      </Stack>
    </Collapse>
  );
}
