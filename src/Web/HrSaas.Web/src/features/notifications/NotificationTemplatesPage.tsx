import AddRoundedIcon from "@mui/icons-material/AddRounded";
import DescriptionRoundedIcon from "@mui/icons-material/DescriptionRounded";
import {
  Box,
  Card,
  CardContent,
  Chip,
  FormControl,
  Grid,
  InputLabel,
  LinearProgress,
  MenuItem,
  Select,
  Stack,
  Typography,
} from "@mui/material";
import { Button } from "@mui/material";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { EmptyState, PageHeader, StatusChip } from "@/components";
import { useNotify } from "@/hooks/useNotify";
import { qk } from "@/lib/query-keys";
import { notificationTemplatesApi } from "./api";
import { CHANNELS } from "./constants";
import { CreateTemplateDialog } from "./CreateTemplateDialog";
import type { CreateTemplateForm } from "./schemas";
import type { NotificationChannel } from "@/types/shared";
import type { NotificationTemplateDto } from "./types";

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

  const templatesQuery = useQuery({
    queryKey: [...qk.notifications.templates, filterChannel || "all"],
    queryFn: () => notificationTemplatesApi.list(filterChannel || undefined),
  });

  const createMutation = useMutation({
    mutationFn: (data: CreateTemplateForm) =>
      notificationTemplatesApi.create({
        ...data,
        description: data.description || undefined,
      }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: qk.notifications.templates });
      setCreateOpen(false);
      notify.success("Template created");
    },
    onError: notify.error,
  });

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

      <CreateTemplateDialog
        open={createOpen}
        onClose={() => setCreateOpen(false)}
        onSubmit={(data) => createMutation.mutate(data)}
        loading={createMutation.isPending}
      />
    </Stack>
  );
}
