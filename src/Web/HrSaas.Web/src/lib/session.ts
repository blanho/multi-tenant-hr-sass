import { jwtDecode } from "jwt-decode";
import type { AuthTokenDto, UserDto } from "../types/api";

const SESSION_KEY = "hrsaas.session";

export interface JwtClaims {
  sub?: string;
  email?: string;
  tenant_id?: string;
  role?: string | string[];
  permissions?: string[];
  exp?: number;
}

export interface SessionState {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: UserDto;
}

export function getSession(): SessionState | null {
  const raw = localStorage.getItem(SESSION_KEY);
  if (!raw) return null;

  try {
    return JSON.parse(raw) as SessionState;
  } catch {
    localStorage.removeItem(SESSION_KEY);
    return null;
  }
}

export function setSession(token: AuthTokenDto): void {
  const session: SessionState = {
    accessToken: token.accessToken,
    refreshToken: token.refreshToken,
    expiresAt: token.expiresAt,
    user: token.user,
  };

  localStorage.setItem(SESSION_KEY, JSON.stringify(session));
  globalThis.dispatchEvent(new StorageEvent("storage", { key: SESSION_KEY }));
}

export function clearSession(): void {
  localStorage.removeItem(SESSION_KEY);
  globalThis.dispatchEvent(new StorageEvent("storage", { key: SESSION_KEY }));
}

export function getAccessToken(): string | null {
  return getSession()?.accessToken ?? null;
}

export function getTenantId(): string | null {
  const session = getSession();
  if (!session) return null;

  try {
    const claims = jwtDecode<JwtClaims>(session.accessToken);
    return claims.tenant_id ?? session.user.tenantId;
  } catch {
    return session.user.tenantId;
  }
}

export function getClaims(): JwtClaims | null {
  const token = getAccessToken();
  if (!token) return null;

  try {
    return jwtDecode<JwtClaims>(token);
  } catch {
    return null;
  }
}

export function hasPermission(permission: string): boolean {
  const claims = getClaims();
  if (!claims) return false;

  const tokenPermissions = claims.permissions ?? [];
  const sessionPermissions = getSession()?.user.permissions ?? [];
  return [...tokenPermissions, ...sessionPermissions].includes(permission);
}
