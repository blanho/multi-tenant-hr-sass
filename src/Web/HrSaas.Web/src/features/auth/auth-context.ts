import { createContext, useContext } from "react";
import type { AuthTokenDto, LoginPayload } from "../../types/api";

export interface AuthContextValue {
  session: AuthTokenDto | null;
  isAuthenticated: boolean;
  isBootstrapping: boolean;
  tenantId: string | null;
  login: (payload: LoginPayload) => Promise<void>;
  logout: () => void;
}

export const AuthContext = createContext<AuthContextValue | undefined>(undefined);

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used within AuthProvider");
  return ctx;
}
