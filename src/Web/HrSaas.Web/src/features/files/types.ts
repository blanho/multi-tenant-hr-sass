import type { FileCategory, FileScanStatus } from "@/types/shared";

export interface FileSummaryDto {
  id: string;
  fileName: string;
  contentType: string;
  fileSize: number;
  category: FileCategory;
  scanStatus: FileScanStatus;
  description: string | null;
  tags: string | null;
  uploadedBy: string;
  entityId: string | null;
  createdAt: string;
}

export interface FileDetailDto extends FileSummaryDto {
  storagePath: string;
  entityType: string | null;
  updatedAt: string | null;
}

export interface FileUploadResult {
  id: string;
  fileName: string;
  fileSize: number;
  contentType: string;
}

export interface FileUrlDto {
  url: string;
  expiresAt: string;
}
