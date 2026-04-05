import { Box, Typography } from "@mui/material";
import ShieldRoundedIcon from "@mui/icons-material/ShieldRounded";
import type { ReactNode } from "react";
import { useAuth } from "@/features/auth/auth-context";
import { hasPermission } from "@/lib/session";

interface RequirePermissionProps {
  permission: string;
  children: ReactNode;
  fallback?: ReactNode;
}

export function RequirePermission({
  permission,
  children,
  fallback,
}: Readonly<RequirePermissionProps>) {
  if (hasPermission(permission)) return <>{children}</>;

  if (fallback) return <>{fallback}</>;

  return (
    <Box sx={{ textAlign: "center", py: 8 }}>
      <ShieldRoundedIcon sx={{ fontSize: 48, color: "text.disabled", mb: 1 }} />
      <Typography variant="h6" color="text.secondary">
        Access Denied
      </Typography>
      <Typography variant="body2" color="text.disabled">
        You do not have the required permission: {permission}
      </Typography>
    </Box>
  );
}

interface RequireRoleProps {
  roles: string[];
  children: ReactNode;
  fallback?: ReactNode;
}

export function RequireRole({
  roles,
  children,
  fallback,
}: Readonly<RequireRoleProps>) {
  const { session } = useAuth();
  const userRole = session?.user.roleName ?? "";

  if (roles.includes(userRole)) return <>{children}</>;

  if (fallback) return <>{fallback}</>;

  return (
    <Box sx={{ textAlign: "center", py: 8 }}>
      <ShieldRoundedIcon sx={{ fontSize: 48, color: "text.disabled", mb: 1 }} />
      <Typography variant="h6" color="text.secondary">
        Access Denied
      </Typography>
      <Typography variant="body2" color="text.disabled">
        This page requires one of these roles: {roles.join(", ")}
      </Typography>
    </Box>
  );
}
