import { CssBaseline, LinearProgress } from "@mui/material";
import { ThemeProvider } from "@mui/material/styles";
import { QueryClientProvider } from "@tanstack/react-query";
import { ReactQueryDevtools } from "@tanstack/react-query-devtools";
import { SnackbarProvider } from "notistack";
import { BrowserRouter, Navigate, Route, Routes, useLocation } from "react-router-dom";
import { queryClient } from "./queryClient";
import { appTheme } from "./theme";
import { AuthProvider } from "../features/auth/AuthContext";
import { useAuth } from "../features/auth/auth-context";
import { LoginPage } from "../features/auth/LoginPage";
import { AppShell } from "../components/layout/AppShell";
import { DashboardPage } from "../features/dashboard/DashboardPage";
import { EmployeesPage } from "../features/employees/EmployeesPage";
import { LeavePage } from "../features/leave/LeavePage";
import { TenantsPage } from "../features/tenants/TenantsPage";
import { BillingPage } from "../features/billing/BillingPage";
import { NotificationsPage } from "../features/notifications/NotificationsPage";

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

export function App() {
  return (
    <ThemeProvider theme={appTheme}>
      <CssBaseline />
      <SnackbarProvider maxSnack={4} autoHideDuration={3500}>
        <QueryClientProvider client={queryClient}>
          <BrowserRouter>
            <AuthProvider>
              <Routes>
                <Route path="/login" element={<LoginPage />} />
                <Route element={<ProtectedLayout />}>
                  <Route path="/" element={<DashboardPage />} />
                  <Route path="/employees" element={<EmployeesPage />} />
                  <Route path="/leave" element={<LeavePage />} />
                  <Route path="/tenants" element={<TenantsPage />} />
                  <Route path="/billing" element={<BillingPage />} />
                  <Route path="/notifications" element={<NotificationsPage />} />
                </Route>
                <Route path="*" element={<Navigate to="/" replace />} />
              </Routes>
            </AuthProvider>
          </BrowserRouter>
          <ReactQueryDevtools initialIsOpen={false} />
        </QueryClientProvider>
      </SnackbarProvider>
    </ThemeProvider>
  );
}
