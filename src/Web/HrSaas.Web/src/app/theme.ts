import { createTheme } from "@mui/material/styles";

export const appTheme = createTheme({
  palette: {
    mode: "light",
    primary: {
      main: "#6366F1",
      light: "#818CF8",
      dark: "#4338CA",
    },
    secondary: {
      main: "#818CF8",
    },
    success: {
      main: "#10B981",
    },
    background: {
      default: "#F5F3FF",
      paper: "#FFFFFF",
    },
    text: {
      primary: "#1E1B4B",
      secondary: "#4338CA",
    },
  },
  shape: {
    borderRadius: 14,
  },
  typography: {
    fontFamily: ["Plus Jakarta Sans", "Inter", "sans-serif"].join(","),
    h4: { fontWeight: 800, letterSpacing: "-0.02em" },
    h5: { fontWeight: 700, letterSpacing: "-0.01em" },
    h6: { fontWeight: 700 },
    subtitle1: { fontWeight: 600 },
    button: { fontWeight: 700, letterSpacing: "0.02em" },
  },
  transitions: {
    duration: {
      shortest: 200,
      shorter: 250,
      short: 300,
    },
  },
  components: {
    MuiButton: {
      defaultProps: { disableElevation: true },
      styleOverrides: {
        root: {
          textTransform: "none",
          fontWeight: 700,
          borderRadius: 12,
          padding: "8px 20px",
          transition: "all 250ms cubic-bezier(0.4,0,0.2,1)",
        },
        containedPrimary: {
          background: "linear-gradient(135deg, #6366F1 0%, #818CF8 100%)",
          "&:hover": {
            background: "linear-gradient(135deg, #4338CA 0%, #6366F1 100%)",
            transform: "translateY(-1px)",
            boxShadow: "0 4px 16px rgba(99,102,241,0.35)",
          },
        },
      },
    },
    MuiCard: {
      styleOverrides: {
        root: {
          border: "1px solid rgba(99,102,241,0.08)",
          boxShadow: "0 8px 32px rgba(99,102,241,0.08)",
          transition: "box-shadow 250ms ease, transform 250ms ease",
          "&:hover": {
            boxShadow: "0 12px 40px rgba(99,102,241,0.14)",
          },
        },
      },
    },
    MuiChip: {
      styleOverrides: {
        root: { fontWeight: 600 },
      },
    },
    MuiDrawer: {
      styleOverrides: {
        paper: {
          background: "linear-gradient(180deg, #F5F3FF 0%, #EDE9FE 100%)",
          borderRight: "1px solid rgba(99,102,241,0.10)",
        },
      },
    },
    MuiListItemButton: {
      styleOverrides: {
        root: {
          borderRadius: 12,
          transition: "all 200ms ease",
          "&.Mui-selected": {
            backgroundColor: "rgba(99,102,241,0.12)",
            color: "#4338CA",
            "& .MuiListItemIcon-root": { color: "#6366F1" },
          },
          "&:hover": {
            backgroundColor: "rgba(99,102,241,0.06)",
          },
        },
      },
    },
    MuiAppBar: {
      styleOverrides: {
        root: {
          backdropFilter: "blur(12px)",
          backgroundColor: "rgba(255,255,255,0.85)",
        },
      },
    },
    MuiLinearProgress: {
      styleOverrides: {
        root: { borderRadius: 4 },
      },
    },
  },
});
