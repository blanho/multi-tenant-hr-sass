import { useEffect, useMemo, useState } from "react";
import type { PropsWithChildren } from "react";
import { api } from "../../lib/api";
import { clearSession, getClaims, getSession, setSession } from "../../lib/session";
import { setAuthHeaders } from "../../lib/http";
import type { AuthTokenDto } from "../../types/api";
import { AuthContext } from "./auth-context";
import type { AuthContextValue } from "./auth-context";

function toAuthTokenDto(value: ReturnType<typeof getSession>): AuthTokenDto | null {
  if (!value) return null;

  return {
    accessToken: value.accessToken,
    refreshToken: value.refreshToken,
    expiresAt: value.expiresAt,
    user: value.user,
  };
}

function resolveTenantId(token: AuthTokenDto | null): string | null {
  const claims = getClaims();
  return claims?.tenant_id ?? token?.user.tenantId ?? null;
}

export function AuthProvider({ children }: Readonly<PropsWithChildren>) {
  const [session, setCurrentSession] = useState<AuthTokenDto | null>(() => {
    const stored = toAuthTokenDto(getSession());
    if (stored) {
      setAuthHeaders(stored.accessToken, resolveTenantId(stored));
    }
    return stored;
  });
  const isBootstrapping = false;

  useEffect(() => {
    const sync = () => {
      const latest = toAuthTokenDto(getSession());
      if (latest) {
        setAuthHeaders(latest.accessToken, resolveTenantId(latest));
      } else {
        setAuthHeaders(null, null);
      }
      setCurrentSession(latest);
    };
    globalThis.addEventListener("storage", sync);

    return () => globalThis.removeEventListener("storage", sync);
  }, []);

  const value = useMemo<AuthContextValue>(() => {
    const tenantId = resolveTenantId(session);

    return {
      session,
      isAuthenticated: !!session?.accessToken,
      isBootstrapping,
      tenantId,
      login: async (payload) => {
        const token = await api.auth.login(payload);
        setSession(token);
        setAuthHeaders(token.accessToken, token.user.tenantId);
        setCurrentSession(token);
      },
      logout: () => {
        clearSession();
        setAuthHeaders(null, null);
        setCurrentSession(null);
      },
    };
  }, [isBootstrapping, session]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
