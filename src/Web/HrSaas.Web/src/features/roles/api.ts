import { http } from "@/lib/http";
import type {
  RoleDto,
  CreateRolePayload,
  UpdateRolePermissionsPayload,
  AssignRolePayload,
} from "./types";

export const rolesApi = {
  list: async () => {
    const { data } = await http.get<RoleDto[]>("/roles");
    return data;
  },
  getById: async (id: string) => {
    const { data } = await http.get<RoleDto>(`/roles/${id}`);
    return data;
  },
  getAvailablePermissions: async () => {
    const { data } = await http.get<string[]>("/roles/permissions");
    return data;
  },
  create: async (payload: CreateRolePayload) => {
    const { data } = await http.post<{ id: string }>("/roles", payload);
    return data;
  },
  updatePermissions: async (id: string, payload: UpdateRolePermissionsPayload) => {
    await http.put(`/roles/${id}/permissions`, payload);
  },
  assign: async (payload: AssignRolePayload) => {
    await http.post("/roles/assign", payload);
  },
  delete: async (id: string) => {
    await http.delete(`/roles/${id}`);
  },
};
