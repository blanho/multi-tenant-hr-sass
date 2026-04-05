import { http } from "@/lib/http";
import type { PagedResult } from "@/types/shared";
import type {
  EmployeeDto,
  EmployeeSummaryDto,
  CreateEmployeePayload,
  UpdateEmployeePayload,
} from "./types";

export const employeesApi = {
  list: async (page = 1, pageSize = 20, department?: string) => {
    const { data } = await http.get<PagedResult<EmployeeSummaryDto>>("/employees", {
      params: { page, pageSize, department: department || undefined },
    });
    return data;
  },
  getById: async (id: string) => {
    const { data } = await http.get<EmployeeDto>(`/employees/${id}`);
    return data;
  },
  create: async (payload: CreateEmployeePayload) => {
    const { data } = await http.post<{ id: string }>("/employees", payload);
    return data;
  },
  update: async (id: string, payload: UpdateEmployeePayload) => {
    await http.put(`/employees/${id}`, payload);
  },
  delete: async (id: string) => {
    await http.delete(`/employees/${id}`);
  },
};
