import { http } from "@/lib/http";
import type { TenantDto, CreateTenantPayload, UpdateTenantPayload } from "./types";

export const tenantsApi = {
  list: async () => {
    const { data } = await http.get<TenantDto[]>("/tenants");
    return data;
  },
  getById: async (id: string) => {
    const { data } = await http.get<TenantDto>(`/tenants/${id}`);
    return data;
  },
  create: async (payload: CreateTenantPayload) => {
    const { data } = await http.post<{ id: string }>("/tenants", payload);
    return data;
  },
  update: async (id: string, payload: UpdateTenantPayload) => {
    await http.put(`/tenants/${id}`, payload);
  },
  suspend: async (id: string, reason: string) => {
    await http.post(`/tenants/${id}/suspend`, { reason });
  },
  reinstate: async (id: string) => {
    await http.post(`/tenants/${id}/reinstate`);
  },
  upgradePlan: async (id: string, newPlan: string) => {
    await http.post(`/tenants/${id}/upgrade-plan`, { newPlan });
  },
};
