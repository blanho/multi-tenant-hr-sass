import { http } from "@/lib/http";
import type { AuthTokenDto, LoginPayload, RegisterPayload } from "./types";

export const authApi = {
  register: async (payload: RegisterPayload) => {
    const { data } = await http.post<AuthTokenDto>("/auth/register", payload);
    return data;
  },
  login: async (payload: LoginPayload) => {
    const { data } = await http.post<AuthTokenDto>("/auth/login", payload);
    return data;
  },
  getMe: async () => {
    const { data } = await http.get<Record<string, string>>("/auth/me");
    return data;
  },
};
