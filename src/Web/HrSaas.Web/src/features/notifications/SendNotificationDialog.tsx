import {
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  Stack,
  TextField,
} from "@mui/material";
import { useState } from "react";
import type {
  NotificationCategory,
  NotificationChannel,
  NotificationPriority,
} from "@/types/api";

const CHANNELS: NotificationChannel[] = ["Email", "Sms", "InApp", "Push", "Webhook", "Slack"];
const CATEGORIES: NotificationCategory[] = [
  "System", "Leave", "Employee", "Billing", "Security", "Tenant", "General",
];
const PRIORITIES: NotificationPriority[] = ["Low", "Normal", "High", "Critical"];

interface SendNotificationDialogProps {
  open: boolean;
  loading: boolean;
  onClose: () => void;
  onSubmit: (payload: {
    userId: string;
    channel: NotificationChannel;
    category: NotificationCategory;
    priority: NotificationPriority;
    subject: string;
    body: string;
  }) => void;
}

export function SendNotificationDialog({
  open,
  loading,
  onClose,
  onSubmit,
}: Readonly<SendNotificationDialogProps>) {
  const [userId, setUserId] = useState("");
  const [channel, setChannel] = useState<NotificationChannel>("InApp");
  const [category, setCategory] = useState<NotificationCategory>("System");
  const [priority, setPriority] = useState<NotificationPriority>("Normal");
  const [subject, setSubject] = useState("");
  const [body, setBody] = useState("");

  const handleSubmit = () => {
    onSubmit({ userId, channel, category, priority, subject, body });
    setUserId("");
    setSubject("");
    setBody("");
  };

  return (
    <Dialog open={open} onClose={onClose} fullWidth maxWidth="sm">
      <DialogTitle>Send Notification</DialogTitle>
      <DialogContent>
        <Stack spacing={2} mt={1}>
          <TextField
            fullWidth
            label="User ID"
            placeholder="Target user GUID"
            value={userId}
            onChange={(e) => setUserId(e.target.value)}
          />
          <Stack direction="row" spacing={2}>
            <FormControl fullWidth>
              <InputLabel>Channel</InputLabel>
              <Select
                value={channel}
                label="Channel"
                onChange={(e) => setChannel(e.target.value as NotificationChannel)}
              >
                {CHANNELS.map((c) => (
                  <MenuItem key={c} value={c}>{c}</MenuItem>
                ))}
              </Select>
            </FormControl>
            <FormControl fullWidth>
              <InputLabel>Category</InputLabel>
              <Select
                value={category}
                label="Category"
                onChange={(e) => setCategory(e.target.value as NotificationCategory)}
              >
                {CATEGORIES.map((c) => (
                  <MenuItem key={c} value={c}>{c}</MenuItem>
                ))}
              </Select>
            </FormControl>
          </Stack>
          <FormControl fullWidth>
            <InputLabel>Priority</InputLabel>
            <Select
              value={priority}
              label="Priority"
              onChange={(e) => setPriority(e.target.value as NotificationPriority)}
            >
              {PRIORITIES.map((p) => (
                <MenuItem key={p} value={p}>{p}</MenuItem>
              ))}
            </Select>
          </FormControl>
          <TextField
            fullWidth
            label="Subject"
            value={subject}
            onChange={(e) => setSubject(e.target.value)}
          />
          <TextField
            fullWidth
            label="Body"
            multiline
            minRows={3}
            value={body}
            onChange={(e) => setBody(e.target.value)}
          />
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
        <Button
          variant="contained"
          onClick={handleSubmit}
          disabled={loading || !userId.trim() || !subject.trim() || !body.trim()}
        >
          {loading ? "Sending..." : "Send"}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
