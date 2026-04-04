import { Box, Stack, Typography } from "@mui/material";
import InboxRoundedIcon from "@mui/icons-material/InboxRounded";
import type { ReactNode } from "react";

interface EmptyStateProps {
  icon?: ReactNode;
  title?: string;
  description?: string;
  action?: ReactNode;
}

export function EmptyState({
  icon,
  title = "No data found",
  description = "There are no records to display.",
  action,
}: Readonly<EmptyStateProps>) {
  return (
    <Box sx={{ textAlign: "center", py: 8 }}>
      <Stack spacing={1.5} alignItems="center">
        {icon ?? (
          <InboxRoundedIcon sx={{ fontSize: 48, color: "text.disabled" }} />
        )}
        <Typography variant="h6" color="text.secondary">
          {title}
        </Typography>
        <Typography variant="body2" color="text.disabled" sx={{ maxWidth: 400 }}>
          {description}
        </Typography>
        {action}
      </Stack>
    </Box>
  );
}
