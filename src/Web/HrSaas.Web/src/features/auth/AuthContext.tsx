import { useEffect, useMemo, useState } from "react";
import type { PropsWithChildren } from "react";
import { api } from "../../lib/api";
import { clearSession, getClaims, getSession, setSession } from "../../lib/session";
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

export function AuthProvider({ children }: Readonly<PropsWithChildren>) {
  const [session, setCurrentSession] = useState<AuthTokenDto | null>(() =>
    toAuthTokenDto(getSession()),
  );
  const isBootstrapping = false;

  useEffect(() => {
    const sync = () => setCurrentSession(toAuthTokenDto(getSession()));
    globalThis.addEventListener("storage", sync);

    return () => globalThis.removeEventListener("storage", sync);
  }, []);

  const value = useMemo<AuthContextValue>(() => {
    const claims = getClaims();

    return {
      session,
      isAuthenticated: !!session?.accessToken,
      isBootstrapping,
      tenantId: claims?.tenant_id ?? session?.user.tenantId ?? null,
      login: async (payload) => {
        const token = await api.login(payload);
        setSession(token);
        setCurrentSession(token);
      },
      logout: () => {
        clearSession();
        setCurrentSession(null);
      },
    };
  }, [isBootstrapping, session]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
