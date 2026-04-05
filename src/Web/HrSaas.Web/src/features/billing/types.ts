import type { BillingCycle } from "@/types/shared";

export interface SubscriptionDto {
  id: string;
  tenantId: string;
  planName: string;
  status: string;
  billingCycle: BillingCycle;
  pricePerCycle: number;
  maxSeats: number;
  usedSeats: number;
  trialEndsAt: string | null;
  currentPeriodEnd: string | null;
  createdAt: string;
}

export interface ActivateSubscriptionPayload {
  price: number;
  cycle: BillingCycle;
  externalId?: string;
}

export interface CancelSubscriptionPayload {
  subscriptionId: string;
  reason: string;
}
