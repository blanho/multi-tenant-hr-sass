import axios, { type AxiosError, type InternalAxiosRequestConfig } from "axios";
import { clearSession, getAccessToken, getRefreshToken, getTenantId, setSession } from "./session";
import type { AuthTokenDto } from "../types/api";

const baseURL =
  import.meta.env.VITE_API_BASE_URL?.trim() || "http://localhost:5000/api/v1";

export const http = axios.create({
  baseURL,
  timeout: 30000,
});

let _cachedAccessToken: string | null = null;
let _cachedTenantId: string | null = null;

export function setAuthHeaders(
  accessToken: string | null,
  tenantId: string | null,
): void {
  _cachedAccessToken = accessToken;
  _cachedTenantId = tenantId;
}

function resetAuth(): void {
  clearSession();
  _cachedAccessToken = null;
  _cachedTenantId = null;
}

let isRefreshing = false;
let failedQueue: Array<{
  resolve: (token: string) => void;
  reject: (error: unknown) => void;
}> = [];

function processQueue(error: unknown, token: string | null) {
  failedQueue.forEach((prom) => {
    if (token) {
      prom.resolve(token);
    } else {
      prom.reject(error);
    }
  });
  failedQueue = [];
}

http.interceptors.request.use((config) => {
  const token = _cachedAccessToken ?? getAccessToken();
  const tenantId = _cachedTenantId ?? getTenantId();

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
  async (error: AxiosError) => {
    const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean };

    if (error.response?.status !== 401 || originalRequest._retry) {
      if (error.response?.status === 401) {
        resetAuth();
      }
      throw error;
    }

    const refreshToken = getRefreshToken();
    if (!refreshToken) {
      resetAuth();
      throw error;
    }

    if (isRefreshing) {
      return new Promise<string>((resolve, reject) => {
        failedQueue.push({ resolve, reject });
      }).then((token) => {
        originalRequest.headers.Authorization = `Bearer ${token}`;
        return http(originalRequest);
      });
    }

    originalRequest._retry = true;
    isRefreshing = true;

    try {
      const { data } = await axios.post<AuthTokenDto>(
        `${baseURL}/auth/refresh`,
        { refreshToken },
      );

      setSession(data);
      setAuthHeaders(data.accessToken, data.user.tenantId);
      processQueue(null, data.accessToken);
      originalRequest.headers.Authorization = `Bearer ${data.accessToken}`;
      return http(originalRequest);
    } catch (refreshError) {
      processQueue(refreshError, null);
      resetAuth();
      throw refreshError;
    } finally {
      isRefreshing = false;
    }
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
