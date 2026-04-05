import AddRoundedIcon from "@mui/icons-material/AddRounded";
import DescriptionRoundedIcon from "@mui/icons-material/DescriptionRounded";
import {
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControl,
  Grid,
  InputLabel,
  LinearProgress,
  MenuItem,
  Select,
  Stack,
  TextField,
  Typography,
} from "@mui/material";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { EmptyState } from "../../components/common/EmptyState";
import { PageHeader } from "../../components/common/PageHeader";
import { StatusChip } from "../../components/common/StatusChip";
import { useNotify } from "../../components/feedback/useNotify";
import { api } from "../../lib/api";
import { qk } from "../../lib/query-keys";
import type {
  NotificationCategory,
  NotificationChannel,
  NotificationTemplateDto,
} from "../../types/api";

const CHANNELS: NotificationChannel[] = ["Email", "Sms", "InApp", "Push", "Webhook", "Slack"];
const CATEGORIES: NotificationCategory[] = [
  "System",
  "Leave",
  "Employee",
  "Billing",
  "Security",
  "Tenant",
  "General",
];

function TemplatesEmpty() {
  return (
    <EmptyState
      title="No Templates"
      description="Create a notification template to get started"
      icon={<DescriptionRoundedIcon sx={{ fontSize: 48 }} />}
    />
  );
}

function TemplateCard({ template }: Readonly<{ template: NotificationTemplateDto }>) {
  return (
    <Card
      sx={{
        height: "100%",
        transition: "box-shadow 200ms, border-color 200ms",
        "&:hover": { borderColor: "primary.main", boxShadow: 6 },
      }}
    >
      <CardContent>
        <Stack spacing={1.5}>
          <Stack direction="row" justifyContent="space-between" alignItems="flex-start">
            <Typography variant="subtitle1" fontWeight={700} noWrap>
              {template.name}
            </Typography>
            <StatusChip status={template.isActive ? "Active" : "Suspended"} />
          </Stack>

          {template.description && (
            <Typography variant="body2" color="text.secondary" sx={{ minHeight: 40 }}>
              {template.description}
            </Typography>
          )}

          <Stack direction="row" spacing={0.75} flexWrap="wrap" useFlexGap>
            <Chip label={template.channel} size="small" variant="outlined" color="primary" />
            <Chip label={template.category} size="small" variant="outlined" />
          </Stack>

          <Box
            sx={{
              p: 1.5,
              borderRadius: 1,
              bgcolor: "grey.50",
              fontFamily: "monospace",
              fontSize: 12,
              whiteSpace: "pre-wrap",
              maxHeight: 80,
              overflow: "auto",
            }}
          >
            <strong>Subject:</strong> {template.subjectTemplate}
          </Box>

          <Box
            sx={{
              p: 1.5,
              borderRadius: 1,
              bgcolor: "grey.50",
              fontFamily: "monospace",
              fontSize: 12,
              whiteSpace: "pre-wrap",
              maxHeight: 120,
              overflow: "auto",
            }}
          >
            {template.bodyTemplate}
          </Box>
        </Stack>
      </CardContent>
    </Card>
  );
}

export function NotificationTemplatesPage() {
  const notify = useNotify();
  const queryClient = useQueryClient();

  const [createOpen, setCreateOpen] = useState(false);
  const [filterChannel, setFilterChannel] = useState<NotificationChannel | "">("");
  const [name, setName] = useState("");
  const [slug, setSlug] = useState("");
  const [description, setDescription] = useState("");
  const [channel, setChannel] = useState<NotificationChannel>("Email");
  const [category, setCategory] = useState<NotificationCategory>("System");
  const [subjectTemplate, setSubjectTemplate] = useState("");
  const [bodyTemplate, setBodyTemplate] = useState("");

  const templatesQuery = useQuery({
    queryKey: [...qk.notifications.templates, filterChannel || "all"],
    queryFn: () =>
      api.notificationTemplates.list(filterChannel || undefined),
  });

  const createMutation = useMutation({
    mutationFn: () =>
      api.notificationTemplates.create({
        name,
        slug,
        channel,
        category,
        subjectTemplate,
        bodyTemplate,
        description: description || undefined,
      }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: qk.notifications.templates });
      resetForm();
      notify.success("Template created");
    },
    onError: notify.error,
  });

  const resetForm = () => {
    setCreateOpen(false);
    setName("");
    setSlug("");
    setDescription("");
    setChannel("Email");
    setCategory("System");
    setSubjectTemplate("");
    setBodyTemplate("");
  };

  const templates = templatesQuery.data ?? [];

  return (
    <Stack spacing={2.5}>
      <PageHeader
        title="Notification Templates"
        subtitle="Manage reusable notification templates for all channels"
        actions={
          <Button
            variant="contained"
            startIcon={<AddRoundedIcon />}
            onClick={() => setCreateOpen(true)}
          >
            Create Template
          </Button>
        }
      />

      <Stack direction="row" spacing={2} alignItems="center">
        <FormControl size="small" sx={{ minWidth: 160 }}>
          <InputLabel>Filter by Channel</InputLabel>
          <Select
            value={filterChannel}
            label="Filter by Channel"
            onChange={(e) => setFilterChannel(e.target.value as NotificationChannel | "")}
          >
            <MenuItem value="">All Channels</MenuItem>
            {CHANNELS.map((c) => (
              <MenuItem key={c} value={c}>
                {c}
              </MenuItem>
            ))}
          </Select>
        </FormControl>
        <Typography variant="body2" color="text.secondary">
          {templates.length} template{templates.length === 1 ? "" : "s"}
        </Typography>
      </Stack>

      {templatesQuery.isFetching && <LinearProgress />}

      {templates.length === 0 && !templatesQuery.isFetching ? (
        <Card>
          <CardContent>
            <TemplatesEmpty />
          </CardContent>
        </Card>
      ) : (
        <Grid container spacing={2}>
          {templates.map((t) => (
            <Grid key={t.id} size={{ xs: 12, md: 6, lg: 4 }}>
              <TemplateCard template={t} />
            </Grid>
          ))}
        </Grid>
      )}

      <Dialog
        open={createOpen}
        onClose={resetForm}
        fullWidth
        maxWidth="sm"
      >
        <DialogTitle>Create Notification Template</DialogTitle>
        <DialogContent>
          <Stack spacing={2} mt={1}>
            <TextField
              fullWidth
              label="Template Name"
              value={name}
              onChange={(e) => setName(e.target.value)}
            />
            <TextField
              fullWidth
              label="Slug"
              placeholder="leave-approval-email"
              value={slug}
              onChange={(e) => setSlug(e.target.value)}
              helperText="Unique identifier for programmatic access"
            />
            <TextField
              fullWidth
              label="Description"
              multiline
              minRows={2}
              value={description}
              onChange={(e) => setDescription(e.target.value)}
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
                    <MenuItem key={c} value={c}>
                      {c}
                    </MenuItem>
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
                    <MenuItem key={c} value={c}>
                      {c}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Stack>
            <TextField
              fullWidth
              label="Subject Template"
              placeholder="Leave {{action}} for {{employee_name}}"
              value={subjectTemplate}
              onChange={(e) => setSubjectTemplate(e.target.value)}
              helperText="Use {{variable}} for dynamic content"
            />
            <TextField
              fullWidth
              label="Body Template"
              multiline
              minRows={4}
              placeholder="Hello {{employee_name}},\n\nYour leave request has been {{action}}."
              value={bodyTemplate}
              onChange={(e) => setBodyTemplate(e.target.value)}
              helperText="Use {{variable}} for dynamic content"
            />
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={resetForm}>Cancel</Button>
          <Button
            variant="contained"
            onClick={() => createMutation.mutate()}
            disabled={
              createMutation.isPending ||
              !name.trim() ||
              !slug.trim() ||
              !subjectTemplate.trim() ||
              !bodyTemplate.trim()
            }
          >
            {createMutation.isPending ? "Creating..." : "Create Template"}
          </Button>
        </DialogActions>
      </Dialog>
    </Stack>
  );
}
