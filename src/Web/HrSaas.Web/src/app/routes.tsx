import { CircularProgress, LinearProgress, Stack } from "@mui/material";
import { lazy, Suspense } from "react";
import { Navigate, Route, Routes, useLocation } from "react-router-dom";
import { ErrorBoundary } from "@/components/common/ErrorBoundary";
import { AppShell } from "@/components/layout/AppShell";
import { useAuth } from "@/features/auth/auth-context";

const LoginPage = lazy(() =>
  import("@/features/auth/LoginPage").then((m) => ({ default: m.LoginPage })),
);
const RegisterPage = lazy(() =>
  import("@/features/auth/RegisterPage").then((m) => ({ default: m.RegisterPage })),
);
const DashboardPage = lazy(() =>
  import("@/features/dashboard/DashboardPage").then((m) => ({ default: m.DashboardPage })),
);
const EmployeesPage = lazy(() =>
  import("@/features/employees/EmployeesPage").then((m) => ({ default: m.EmployeesPage })),
);
const LeavePage = lazy(() =>
  import("@/features/leave/LeavePage").then((m) => ({ default: m.LeavePage })),
);
const TenantsPage = lazy(() =>
  import("@/features/tenants/TenantsPage").then((m) => ({ default: m.TenantsPage })),
);
const BillingPage = lazy(() =>
  import("@/features/billing/BillingPage").then((m) => ({ default: m.BillingPage })),
);
const NotificationsPage = lazy(() =>
  import("@/features/notifications/NotificationsPage").then((m) => ({
    default: m.NotificationsPage,
  })),
);
const NotificationPreferencesPage = lazy(() =>
  import("@/features/notifications/NotificationPreferencesPage").then((m) => ({
    default: m.NotificationPreferencesPage,
  })),
);
const NotificationTemplatesPage = lazy(() =>
  import("@/features/notifications/NotificationTemplatesPage").then((m) => ({
    default: m.NotificationTemplatesPage,
  })),
);
const UsersPage = lazy(() =>
  import("@/features/users/UsersPage").then((m) => ({ default: m.UsersPage })),
);
const RolesPage = lazy(() =>
  import("@/features/roles/RolesPage").then((m) => ({ default: m.RolesPage })),
);
const AuditLogsPage = lazy(() =>
  import("@/features/audit/AuditLogsPage").then((m) => ({ default: m.AuditLogsPage })),
);
const FilesPage = lazy(() =>
  import("@/features/files/FilesPage").then((m) => ({ default: m.FilesPage })),
);

function PageLoader() {
  return (
    <Stack alignItems="center" justifyContent="center" sx={{ py: 12 }}>
      <CircularProgress />
    </Stack>
  );
}

function ProtectedLayout() {
  const { isAuthenticated, isBootstrapping } = useAuth();
  const location = useLocation();

  if (isBootstrapping) {
    return <LinearProgress />;
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace state={{ from: location.pathname }} />;
  }

  return <AppShell />;
}

export function AppRoutes() {
  return (
    <ErrorBoundary>
      <Suspense fallback={<PageLoader />}>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
          <Route element={<ProtectedLayout />}>
            <Route path="/" element={<DashboardPage />} />
            <Route path="/employees" element={<EmployeesPage />} />
            <Route path="/leave" element={<LeavePage />} />
            <Route path="/tenants" element={<TenantsPage />} />
            <Route path="/billing" element={<BillingPage />} />
            <Route path="/notifications" element={<NotificationsPage />} />
            <Route path="/notification-preferences" element={<NotificationPreferencesPage />} />
            <Route path="/notification-templates" element={<NotificationTemplatesPage />} />
            <Route path="/users" element={<UsersPage />} />
            <Route path="/roles" element={<RolesPage />} />
            <Route path="/audit-logs" element={<AuditLogsPage />} />
            <Route path="/files" element={<FilesPage />} />
          </Route>
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </Suspense>
    </ErrorBoundary>
  );
}
