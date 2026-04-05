import { CssBaseline } from "@mui/material";
import { ThemeProvider } from "@mui/material/styles";
import { QueryClientProvider } from "@tanstack/react-query";
import { ReactQueryDevtools } from "@tanstack/react-query-devtools";
import { SnackbarProvider } from "notistack";
import { BrowserRouter } from "react-router-dom";
import { AuthProvider } from "@/features/auth/AuthContext";
import { queryClient } from "./queryClient";
import { AppRoutes } from "./routes";
import { appTheme } from "./theme";

export function App() {
  return (
    <ThemeProvider theme={appTheme}>
      <CssBaseline />
      <SnackbarProvider maxSnack={4} autoHideDuration={3500}>
        <QueryClientProvider client={queryClient}>
          <BrowserRouter>
            <AuthProvider>
              <AppRoutes />
            </AuthProvider>
          </BrowserRouter>
          <ReactQueryDevtools initialIsOpen={false} />
        </QueryClientProvider>
      </SnackbarProvider>
    </ThemeProvider>
  );
}
