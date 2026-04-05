import { Drawer, Stack, Typography } from "@mui/material";
import { DetailRow, JsonBlock, StatusChip } from "@/components";
import dayjs from "dayjs";
import type { AuditLogDetailDto } from "./types";

interface AuditDetailDrawerProps {
  open: boolean;
  onClose: () => void;
  log: AuditLogDetailDto | null;
}

export function AuditDetailDrawer({
  open,
  onClose,
  log,
}: Readonly<AuditDetailDrawerProps>) {
  return (
    <Drawer
      anchor="right"
      open={open}
      onClose={onClose}
      slotProps={{ paper: { sx: { width: 440, p: 3 } } }}
    >
      {log && (
        <Stack spacing={2}>
          <Typography variant="h6">Audit Log Detail</Typography>
          <StatusChip status={log.severity} />
          <Stack spacing={0.5}>
            <DetailRow label="Action" value={log.action} />
            <DetailRow label="Entity" value={`${log.entityType} / ${log.entityId}`} />
            <DetailRow label="Description" value={log.description} />
            <DetailRow label="User" value={log.userEmail ?? "System"} />
            <DetailRow label="IP Address" value={log.ipAddress ?? "—"} />
            <DetailRow label="Duration" value={`${log.durationMs}ms`} />
            <DetailRow
              label="Timestamp"
              value={dayjs(log.createdAt).format("MMM D, YYYY h:mm:ss A")}
            />
            {log.requestMethod && log.requestPath && (
              <DetailRow
                label="Request"
                value={`${log.requestMethod} ${log.requestPath}`}
              />
            )}
            {log.correlationId && (
              <DetailRow label="Correlation ID" value={log.correlationId} />
            )}
          </Stack>

          {log.oldValues && <JsonBlock title="Old Values" json={log.oldValues} />}
          {log.newValues && <JsonBlock title="New Values" json={log.newValues} />}
          {log.additionalData && (
            <JsonBlock title="Additional Data" json={log.additionalData} />
          )}
        </Stack>
      )}
    </Drawer>
  );
}
