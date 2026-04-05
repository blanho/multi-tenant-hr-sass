import SwapHorizRoundedIcon from "@mui/icons-material/SwapHorizRounded";
import { Box, Button, Chip, Drawer, Stack, Typography } from "@mui/material";
import { DetailRow, StatusChip } from "@/components";
import dayjs from "dayjs";
import type { UserDto } from "@/types/api";

interface UserDetailDrawerProps {
  open: boolean;
  onClose: () => void;
  user: UserDto | null;
  onChangeRole: (user: UserDto) => void;
}

export function UserDetailDrawer({
  open,
  onClose,
  user,
  onChangeRole,
}: Readonly<UserDetailDrawerProps>) {
  return (
    <Drawer
      anchor="right"
      open={open}
      onClose={onClose}
      slotProps={{ paper: { sx: { width: 380, p: 3 } } }}
    >
      {user && (
        <Stack spacing={2}>
          <Typography variant="h6">User Details</Typography>
          <StatusChip status={user.isActive ? "Active" : "Suspended"} />
          <Stack spacing={1} mt={1}>
            <DetailRow label="Email" value={user.email} />
            <DetailRow label="Role" value={user.roleName} />
            <DetailRow label="Joined" value={dayjs(user.createdAt).format("MMM D, YYYY")} />
            <DetailRow label="User ID" value={user.id} />
            <DetailRow label="Role ID" value={user.roleId} />
          </Stack>
          <Button
            variant="outlined"
            size="small"
            startIcon={<SwapHorizRoundedIcon />}
            onClick={() => {
              onClose();
              onChangeRole(user);
            }}
          >
            Change Role
          </Button>
          <Typography variant="subtitle2" mt={2}>
            Permissions ({user.permissions.length})
          </Typography>
          <Box sx={{ display: "flex", flexWrap: "wrap", gap: 0.5 }}>
            {user.permissions.map((p) => (
              <Chip key={p} label={p} size="small" variant="outlined" />
            ))}
            {user.permissions.length === 0 && (
              <Typography variant="body2" color="text.secondary">
                No permissions assigned
              </Typography>
            )}
          </Box>
        </Stack>
      )}
    </Drawer>
  );
}
