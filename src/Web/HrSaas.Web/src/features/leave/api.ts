import { http } from "@/lib/http";
import type { LeaveRequestDto, LeaveBalanceDto, ApplyLeavePayload } from "./types";

export const leaveApi = {
  getById: async (id: string) => {
    const { data } = await http.get<LeaveRequestDto>(`/leave/${id}`);
    return data;
  },
  getByEmployee: async (employeeId: string, year?: number) => {
    const { data } = await http.get<LeaveRequestDto[]>(`/leave/employee/${employeeId}`, {
      params: { year: year || undefined },
    });
    return data;
  },
  getBalance: async (employeeId: string, year?: number) => {
    const { data } = await http.get<LeaveBalanceDto>(`/leave/balance/${employeeId}`, {
      params: { year: year || undefined },
    });
    return data;
  },
  getPending: async () => {
    const { data } = await http.get<LeaveRequestDto[]>("/leave/pending");
    return data;
  },
  apply: async (payload: ApplyLeavePayload) => {
    const { data } = await http.post<{ id: string }>("/leave", payload);
    return data;
  },
  approve: async (leaveId: string) => {
    await http.post(`/leave/${leaveId}/approve`);
  },
  reject: async (leaveId: string, note: string) => {
    await http.post(`/leave/${leaveId}/reject`, { note });
  },
  cancel: async (leaveId: string) => {
    await http.delete(`/leave/${leaveId}`);
  },
};
