import { http } from "@/lib/http";
import type { PagedResult, FileCategory, FileScanStatus } from "@/types/shared";
import type {
  FileSummaryDto,
  FileDetailDto,
  FileUploadResult,
  FileUrlDto,
} from "./types";

export const filesApi = {
  upload: async (
    file: File,
    category: FileCategory = "General",
    description?: string,
    entityId?: string,
    entityType?: string,
  ) => {
    const formData = new FormData();
    formData.append("file", file);
    formData.append("category", category);
    if (description) formData.append("description", description);
    if (entityId) formData.append("entityId", entityId);
    if (entityType) formData.append("entityType", entityType);

    const { data } = await http.post<FileUploadResult>("/files", formData, {
      headers: { "Content-Type": "multipart/form-data" },
    });
    return data;
  },
  getById: async (id: string) => {
    const { data } = await http.get<FileDetailDto>(`/files/${id}`);
    return data;
  },
  list: async (params: {
    page?: number;
    pageSize?: number;
    category?: FileCategory;
    scanStatus?: FileScanStatus;
    entityId?: string;
    entityType?: string;
  } = {}) => {
    const { data } = await http.get<PagedResult<FileSummaryDto>>("/files", {
      params: {
        page: params.page ?? 1,
        pageSize: params.pageSize ?? 20,
        category: params.category || undefined,
        scanStatus: params.scanStatus || undefined,
        entityId: params.entityId || undefined,
        entityType: params.entityType || undefined,
      },
    });
    return data;
  },
  getMetadata: async (id: string) => {
    const { data } = await http.get<FileDetailDto>(`/files/${id}/metadata`);
    return data;
  },
  download: (id: string) => `${http.defaults.baseURL}/files/${id}/download`,
  getUrl: async (id: string, expiresInMinutes = 60) => {
    const { data } = await http.get<FileUrlDto>(`/files/${id}/url`, {
      params: { expiresInMinutes },
    });
    return data;
  },
  delete: async (id: string) => {
    await http.delete(`/files/${id}`);
  },
};
