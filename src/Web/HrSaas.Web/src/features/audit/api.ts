import { http } from "@/lib/http";
import type { PagedResult, AuditAction, AuditEntityType, AuditSeverity } from "@/types/shared";
import type { AuditLogSummaryDto, AuditLogDetailDto } from "./types";

export const auditLogsApi = {
  list: async (params: {
    page?: number;
    pageSize?: number;
    action?: AuditAction;
    entityType?: AuditEntityType;
    userId?: string;
    entityId?: string;
    severity?: AuditSeverity;
    from?: string;
    to?: string;
    isSystemGenerated?: boolean;
  } = {}) => {
    const { data } = await http.get<PagedResult<AuditLogSummaryDto>>("/auditlogs", {
      params: {
        page: params.page ?? 1,
        pageSize: params.pageSize ?? 50,
        action: params.action || undefined,
        entityType: params.entityType || undefined,
        userId: params.userId || undefined,
        entityId: params.entityId || undefined,
        severity: params.severity || undefined,
        from: params.from || undefined,
        to: params.to || undefined,
        isSystemGenerated: params.isSystemGenerated ?? undefined,
      },
    });
    return data;
  },
  getById: async (id: string) => {
    const { data } = await http.get<AuditLogDetailDto>(`/auditlogs/${id}`);
    return data;
  },
  getByEntity: async (
    entityType: AuditEntityType,
    entityId: string,
    page = 1,
    pageSize = 50,
  ) => {
    const { data } = await http.get<PagedResult<AuditLogSummaryDto>>(
      `/auditlogs/entity/${entityType}/${entityId}`,
      { params: { page, pageSize } },
    );
    return data;
  },
};
