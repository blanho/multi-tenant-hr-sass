import { createTheme } from "@mui/material/styles";

export const appTheme = createTheme({
  palette: {
    mode: "light",
    primary: {
      main: "#7C3AED",
      light: "#A78BFA",
      dark: "#4C1D95",
    },
    secondary: {
      main: "#F97316",
    },
    background: {
      default: "#FAF5FF",
      paper: "#FFFFFF",
    },
  },
  shape: {
    borderRadius: 12,
  },
  typography: {
    fontFamily: ["Inter", "Source Sans 3", "Roboto", "sans-serif"].join(","),
    h5: {
      fontWeight: 700,
    },
    h6: {
      fontWeight: 700,
    },
  },
  components: {
    MuiButton: {
      defaultProps: {
        disableElevation: true,
      },
      styleOverrides: {
        root: {
          textTransform: "none",
          fontWeight: 600,
        },
      },
    },
    MuiCard: {
      styleOverrides: {
        root: {
          border: "1px solid rgba(76,29,149,0.08)",
          boxShadow: "0 8px 24px rgba(76,29,149,0.08)",
        },
      },
    },
  },
});
