import {
  AppBar,
  Box,
  Chip,
  Divider,
  Drawer,
  IconButton,
  List,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Stack,
  Toolbar,
  Typography,
} from "@mui/material";
import { Link as RouterLink, Outlet, useLocation } from "react-router-dom";
import DashboardRoundedIcon from "@mui/icons-material/DashboardRounded";
import GroupsRoundedIcon from "@mui/icons-material/GroupsRounded";
import EventRoundedIcon from "@mui/icons-material/EventRounded";
import ApartmentRoundedIcon from "@mui/icons-material/ApartmentRounded";
import CreditCardRoundedIcon from "@mui/icons-material/CreditCardRounded";
import NotificationsRoundedIcon from "@mui/icons-material/NotificationsRounded";
import LogoutRoundedIcon from "@mui/icons-material/LogoutRounded";
import { useAuth } from "../../features/auth/auth-context";

const drawerWidth = 260;

const navItems = [
  { label: "Dashboard", path: "/", icon: <DashboardRoundedIcon /> },
  { label: "Employees", path: "/employees", icon: <GroupsRoundedIcon /> },
  { label: "Leave", path: "/leave", icon: <EventRoundedIcon /> },
  { label: "Tenants", path: "/tenants", icon: <ApartmentRoundedIcon /> },
  { label: "Billing", path: "/billing", icon: <CreditCardRoundedIcon /> },
  { label: "Notifications", path: "/notifications", icon: <NotificationsRoundedIcon /> },
];

export function AppShell() {
  const location = useLocation();
  const { logout, session, tenantId } = useAuth();

  return (
    <Box sx={{ display: "flex", minHeight: "100vh" }}>
      <Drawer
        variant="permanent"
        sx={{
          width: drawerWidth,
          flexShrink: 0,
          ["& .MuiDrawer-paper"]: {
            width: drawerWidth,
            boxSizing: "border-box",
          },
        }}
      >
        <Toolbar>
          <Stack>
            <Typography variant="h6">HrSaas</Typography>
            <Typography variant="caption" color="text.secondary">
              Multi-tenant HR Suite
            </Typography>
          </Stack>
        </Toolbar>
        <Divider />
        <List sx={{ p: 1 }}>
          {navItems.map((item) => (
            <ListItemButton
              key={item.path}
              component={RouterLink}
              to={item.path}
              selected={location.pathname === item.path}
              sx={{ borderRadius: 2, mb: 0.5 }}
            >
              <ListItemIcon>{item.icon}</ListItemIcon>
              <ListItemText primary={item.label} />
            </ListItemButton>
          ))}
        </List>
      </Drawer>

      <Box sx={{ flexGrow: 1 }}>
        <AppBar position="sticky" color="transparent" elevation={0} sx={{ borderBottom: "1px solid", borderColor: "divider" }}>
          <Toolbar>
            <Stack direction="row" spacing={1.5} alignItems="center" sx={{ flexGrow: 1 }}>
              <Typography variant="body2" color="text.secondary">
                {session?.user.email}
              </Typography>
              <Chip size="small" color="primary" label={`Tenant: ${tenantId ?? "N/A"}`} />
            </Stack>
            <IconButton color="primary" onClick={logout}>
              <LogoutRoundedIcon />
            </IconButton>
          </Toolbar>
        </AppBar>

        <Box component="main" sx={{ p: 3 }}>
          <Outlet />
        </Box>
      </Box>
    </Box>
  );
}
