import type { AuditAction, AuditEntityType, AuditSeverity } from "@/types/shared";

export interface AuditLogSummaryDto {
  id: string;
  tenantId: string;
  userId: string | null;
  userEmail: string | null;
  action: AuditAction;
  entityType: AuditEntityType;
  entityId: string;
  description: string;
  ipAddress: string | null;
  severity: AuditSeverity;
  correlationId: string | null;
  isSystemGenerated: boolean;
  tags: string | null;
  userAgent: string | null;
  durationMs: number;
  createdAt: string;
}

export interface AuditLogDetailDto extends AuditLogSummaryDto {
  oldValues: string | null;
  newValues: string | null;
  additionalData: string | null;
  requestPath: string | null;
  requestMethod: string | null;
}
