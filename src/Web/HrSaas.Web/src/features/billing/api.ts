import { http } from "@/lib/http";
import type {
  SubscriptionDto,
  ActivateSubscriptionPayload,
  CancelSubscriptionPayload,
} from "./types";

export const billingApi = {
  getSubscription: async () => {
    const { data } = await http.get<SubscriptionDto>("/billing/subscription");
    return data;
  },
  createFree: async () => {
    const { data } = await http.post<{ id: string }>("/billing/subscription/create-free");
    return data;
  },
  activate: async (subscriptionId: string, payload: ActivateSubscriptionPayload) => {
    await http.post(`/billing/subscription/${subscriptionId}/activate`, payload);
  },
  cancel: async (payload: CancelSubscriptionPayload) => {
    await http.post("/billing/subscription/cancel", payload);
  },
};
