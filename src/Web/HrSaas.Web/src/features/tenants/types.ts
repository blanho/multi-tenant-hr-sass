import type { TenantPlan, TenantStatus } from "@/types/shared";

export interface TenantDto {
  id: string;
  name: string;
  slug: string;
  contactEmail: string;
  plan: TenantPlan;
  status: TenantStatus;
  maxEmployees: number;
  createdAt: string;
}

export interface CreateTenantPayload {
  name: string;
  slug: string;
  contactEmail: string;
  plan?: TenantPlan;
}

export interface UpdateTenantPayload {
  name: string;
  contactEmail: string;
}
