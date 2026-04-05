import type {
  DigestFrequency,
  NotificationCategory,
  NotificationChannel,
  NotificationPriority,
  NotificationStatus,
} from "@/types/shared";

export interface NotificationSummaryDto {
  id: string;
  userId: string;
  tenantId: string;
  channel: NotificationChannel;
  category: NotificationCategory;
  priority: NotificationPriority;
  subject: string;
  body: string;
  status: NotificationStatus;
  createdAt: string;
  readAt: string | null;
  deliveredAt: string | null;
  correlationId: string | null;
}

export interface NotificationDetailDto extends NotificationSummaryDto {
  scheduledFor: string | null;
  expiresAt: string | null;
  metadata: string | null;
  templateId: string | null;
  retryCount: number;
  maxRetries: number;
  groupKey: string | null;
  updatedAt: string | null;
}

export interface NotificationStatsDto {
  totalCount: number;
  readCount: number;
  unreadCount: number;
  sentCount: number;
  deliveredCount: number;
  failedCount: number;
}

export interface NotificationPreferencesDto {
  id: string;
  userId: string;
  tenantId: string;
  enabledChannels: NotificationChannel[];
  mutedCategories: NotificationCategory[];
  emailEnabled: boolean;
  digestFrequency: DigestFrequency;
  quietHoursStart: string | null;
  quietHoursEnd: string | null;
  timezone: string | null;
}

export interface NotificationTemplateDto {
  id: string;
  name: string;
  description: string;
  channel: NotificationChannel;
  category: NotificationCategory;
  subjectTemplate: string;
  bodyTemplate: string;
  isActive: boolean;
  metadata: string | null;
}

export interface CreateNotificationPayload {
  userId: string;
  channel: NotificationChannel;
  category: NotificationCategory;
  priority: NotificationPriority;
  subject: string;
  body: string;
  recipient?: string;
  groupKey?: string;
  scheduledFor?: string;
  metadata?: string;
  templateId?: string;
  expiresAt?: string;
}

export interface CreateBulkNotificationPayload {
  userIds: string[];
  channel: NotificationChannel;
  category: NotificationCategory;
  priority: NotificationPriority;
  subject: string;
  body: string;
  recipientAddresses?: string[];
  correlationId?: string;
  metadata?: string;
}

export interface UpdateNotificationPreferencesPayload {
  enabledChannels: NotificationChannel[];
  mutedCategories: NotificationCategory[];
  emailEnabled: boolean;
  digestFrequency: DigestFrequency;
  quietHoursStart?: string;
  quietHoursEnd?: string;
  timezone?: string;
}

export interface CreateNotificationTemplatePayload {
  name: string;
  slug: string;
  channel: NotificationChannel;
  category: NotificationCategory;
  subjectTemplate: string;
  bodyTemplate: string;
  description?: string;
  samplePayload?: string;
}
