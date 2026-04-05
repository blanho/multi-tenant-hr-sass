import { Drawer, Stack, Typography } from "@mui/material";
import { DetailRow, StatusChip } from "@/components";
import dayjs from "dayjs";
import type { FileDetailDto } from "./types";
import { formatFileSize } from "./utils";

interface FileDetailDrawerProps {
  open: boolean;
  onClose: () => void;
  file: FileDetailDto | null;
}

export function FileDetailDrawer({
  open,
  onClose,
  file,
}: Readonly<FileDetailDrawerProps>) {
  return (
    <Drawer
      anchor="right"
      open={open}
      onClose={onClose}
      slotProps={{ paper: { sx: { width: 380, p: 3 } } }}
    >
      {file && (
        <Stack spacing={2}>
          <Typography variant="h6">File Details</Typography>
          <StatusChip status={file.scanStatus} />
          <Stack spacing={0.5}>
            <DetailRow label="Name" value={file.fileName} />
            <DetailRow label="Type" value={file.contentType} />
            <DetailRow label="Size" value={formatFileSize(file.fileSize)} />
            <DetailRow label="Category" value={file.category} />
            <DetailRow label="Uploaded By" value={file.uploadedBy} />
            <DetailRow
              label="Uploaded"
              value={dayjs(file.createdAt).format("MMM D, YYYY")}
            />
            {file.description && (
              <DetailRow label="Description" value={file.description} />
            )}
            {file.tags && <DetailRow label="Tags" value={file.tags} />}
          </Stack>
        </Stack>
      )}
    </Drawer>
  );
}
