import CloudUploadRoundedIcon from "@mui/icons-material/CloudUploadRounded";
import DeleteRoundedIcon from "@mui/icons-material/DeleteRounded";
import DownloadRoundedIcon from "@mui/icons-material/DownloadRounded";
import FolderOpenRoundedIcon from "@mui/icons-material/FolderOpenRounded";
import InfoRoundedIcon from "@mui/icons-material/InfoRounded";
import {
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  Collapse,
  FormControl,
  IconButton,
  InputLabel,
  MenuItem,
  Select,
  Stack,
  Tooltip,
} from "@mui/material";
import type { GridColDef, GridPaginationModel } from "@mui/x-data-grid";
import { DataGrid } from "@mui/x-data-grid";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import dayjs from "dayjs";
import { useCallback, useMemo, useState } from "react";
import { ConfirmDialog, EmptyState, PageHeader, StatusChip } from "@/components";
import { useNotify } from "@/hooks/useNotify";
import { api } from "@/lib/api";
import { qk } from "@/lib/query-keys";
import type { FileCategory, FileScanStatus } from "@/types/shared";
import type { FileDetailDto, FileSummaryDto } from "./types";
import { FILE_CATEGORIES, SCAN_STATUSES } from "./constants";
import { formatFileSize } from "./utils";
import { FileUploadDialog } from "./FileUploadDialog";
import { FileDetailDrawer } from "./FileDetailDrawer";

function FilesEmpty() {
  return (
    <EmptyState
      title="No Files"
      description="Upload your first file to get started"
      icon={<FolderOpenRoundedIcon sx={{ fontSize: 48 }} />}
    />
  );
}

export function FilesPage() {
  const notify = useNotify();
  const queryClient = useQueryClient();

  const [paginationModel, setPaginationModel] = useState<GridPaginationModel>({
    page: 0,
    pageSize: 20,
  });
  const [categoryFilter, setCategoryFilter] = useState<FileCategory | "">("");
  const [scanFilter, setScanFilter] = useState<FileScanStatus | "">("");
  const [filtersOpen, setFiltersOpen] = useState(false);
  const [uploadOpen, setUploadOpen] = useState(false);
  const [detailOpen, setDetailOpen] = useState(false);
  const [selected, setSelected] = useState<FileDetailDto | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<FileSummaryDto | null>(null);

  const filterParams = useMemo(
    () => ({
      page: paginationModel.page + 1,
      pageSize: paginationModel.pageSize,
      category: categoryFilter || undefined,
      scanStatus: scanFilter || undefined,
    }),
    [paginationModel.page, paginationModel.pageSize, categoryFilter, scanFilter],
  );

  const listQuery = useQuery({
    queryKey: qk.files.list(filterParams),
    queryFn: () => api.files.list(filterParams),
  });

  const uploadMutation = useMutation({
    mutationFn: ({ file, category, description }: { file: File; category: FileCategory; description?: string }) =>
      api.files.upload(file, category, description),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ["files"] });
      setUploadOpen(false);
      notify.success("File uploaded successfully");
    },
    onError: notify.error,
  });

  const deleteMutation = useMutation({
    mutationFn: () => {
      if (!deleteTarget) return Promise.reject(new Error("No file selected"));
      return api.files.delete(deleteTarget.id);
    },
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ["files"] });
      setDeleteTarget(null);
      notify.success("File deleted");
    },
    onError: notify.error,
  });

  const openDetail = useCallback(
    async (id: string) => {
      try {
        const detail = await api.files.getById(id);
        setSelected(detail);
        setDetailOpen(true);
      } catch {
        notify.error("Failed to load file details");
      }
    },
    [notify],
  );

  const handleDownload = useCallback(
    async (id: string) => {
      try {
        const { url } = await api.files.getUrl(id);
        window.open(url, "_blank");
      } catch {
        notify.error("Failed to get download URL");
      }
    },
    [notify],
  );

  const columns = useMemo<GridColDef<FileSummaryDto>[]>(
    () => [
      { field: "fileName", headerName: "File Name", flex: 1, minWidth: 200 },
      {
        field: "category",
        headerName: "Category",
        width: 120,
        renderCell: ({ value }) => <Chip label={value} size="small" variant="outlined" />,
      },
      {
        field: "fileSize",
        headerName: "Size",
        width: 100,
        valueFormatter: (value: number) => formatFileSize(value),
      },
      {
        field: "scanStatus",
        headerName: "Scan",
        width: 100,
        renderCell: ({ value }) => <StatusChip status={value} />,
      },
      {
        field: "createdAt",
        headerName: "Uploaded",
        width: 150,
        valueFormatter: (value: string) => dayjs(value).format("MMM D, YYYY"),
      },
      {
        field: "actions",
        headerName: "",
        width: 130,
        sortable: false,
        filterable: false,
        renderCell: ({ row }) => (
          <Stack direction="row" spacing={0.5}>
            <Tooltip title="Details">
              <IconButton size="small" onClick={() => void openDetail(row.id)}>
                <InfoRoundedIcon fontSize="small" />
              </IconButton>
            </Tooltip>
            <Tooltip title="Download">
              <IconButton size="small" onClick={() => void handleDownload(row.id)}>
                <DownloadRoundedIcon fontSize="small" />
              </IconButton>
            </Tooltip>
            <Tooltip title="Delete">
              <IconButton size="small" color="error" onClick={() => setDeleteTarget(row)}>
                <DeleteRoundedIcon fontSize="small" />
              </IconButton>
            </Tooltip>
          </Stack>
        ),
      },
    ],
    [openDetail, handleDownload],
  );

  const rows = listQuery.data?.items ?? [];
  const rowCount = listQuery.data?.totalCount ?? 0;

  return (
    <Stack spacing={2.5}>
      <PageHeader
        title="Files"
        subtitle="Upload, manage, and download files"
        actions={
          <Button
            variant="contained"
            startIcon={<CloudUploadRoundedIcon />}
            onClick={() => setUploadOpen(true)}
          >
            Upload File
          </Button>
        }
      />

      <Card>
        <CardContent>
          <Stack direction="row" justifyContent="flex-end" mb={1}>
            <Button size="small" onClick={() => setFiltersOpen((o) => !o)}>
              Filters
            </Button>
          </Stack>

          <Collapse in={filtersOpen}>
            <Stack direction="row" spacing={2} mb={2}>
              <FormControl size="small" sx={{ minWidth: 140 }}>
                <InputLabel>Category</InputLabel>
                <Select
                  value={categoryFilter}
                  label="Category"
                  onChange={(e) => {
                    setCategoryFilter(e.target.value as FileCategory | "");
                    setPaginationModel((m) => ({ ...m, page: 0 }));
                  }}
                >
                  <MenuItem value="">All</MenuItem>
                  {FILE_CATEGORIES.map((c) => (
                    <MenuItem key={c} value={c}>
                      {c}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>

              <FormControl size="small" sx={{ minWidth: 140 }}>
                <InputLabel>Scan Status</InputLabel>
                <Select
                  value={scanFilter}
                  label="Scan Status"
                  onChange={(e) => {
                    setScanFilter(e.target.value as FileScanStatus | "");
                    setPaginationModel((m) => ({ ...m, page: 0 }));
                  }}
                >
                  <MenuItem value="">All</MenuItem>
                  {SCAN_STATUSES.map((s) => (
                    <MenuItem key={s} value={s}>
                      {s}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Stack>
          </Collapse>

          <Box sx={{ height: 520 }}>
            <DataGrid
              rows={rows}
              columns={columns}
              rowCount={rowCount}
              loading={listQuery.isFetching}
              paginationMode="server"
              paginationModel={paginationModel}
              onPaginationModelChange={setPaginationModel}
              pageSizeOptions={[10, 20, 50]}
              disableRowSelectionOnClick
              slots={{ noRowsOverlay: FilesEmpty }}
              slotProps={{ noRowsOverlay: {} }}
              density="compact"
              sx={{ border: 0 }}
            />
          </Box>
        </CardContent>
      </Card>

      <FileUploadDialog
        open={uploadOpen}
        onClose={() => setUploadOpen(false)}
        onUpload={(file, category, description) =>
          uploadMutation.mutate({ file, category, description })
        }
        loading={uploadMutation.isPending}
      />

      <FileDetailDrawer
        open={detailOpen}
        onClose={() => setDetailOpen(false)}
        file={selected}
      />

      <ConfirmDialog
        open={!!deleteTarget}
        title="Delete File"
        message={`Are you sure you want to delete "${deleteTarget?.fileName}"? This action cannot be undone.`}
        confirmLabel="Delete"
        severity="error"
        loading={deleteMutation.isPending}
        onConfirm={() => deleteMutation.mutate()}
        onCancel={() => setDeleteTarget(null)}
      />
    </Stack>
  );
}
