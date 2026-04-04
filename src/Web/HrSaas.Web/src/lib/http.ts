import axios from "axios";
import { clearSession, getAccessToken, getTenantId } from "./session";

const baseURL =
  import.meta.env.VITE_API_BASE_URL?.trim() || "http://localhost:5000/api/v1";

export const http = axios.create({
  baseURL,
  timeout: 30000,
});

http.interceptors.request.use((config) => {
  const token = getAccessToken();
  const tenantId = getTenantId();

  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }

  if (tenantId) {
    config.headers["X-Tenant-ID"] = tenantId;
  }

  return config;
});

http.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      clearSession();
    }

    return Promise.reject(error);
  },
);

export function extractErrorMessage(error: unknown): string {
  if (!axios.isAxiosError(error)) {
    return "Unexpected error occurred";
  }

  const data = error.response?.data as
    | { error?: string; detail?: string; title?: string; errors?: string[] }
    | undefined;

  if (Array.isArray(data?.errors) && data.errors.length > 0) {
    return data.errors.join("\n");
  }

  return data?.error ?? data?.detail ?? data?.title ?? error.message;
}
