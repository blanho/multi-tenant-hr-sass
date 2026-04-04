import { Card, CardContent, Stack, Typography } from "@mui/material";
import type { ReactNode } from "react";

interface StatCardProps {
  label: string;
  value: string | number;
  helper?: string;
  icon?: ReactNode;
}

export function StatCard({ label, value, helper, icon }: Readonly<StatCardProps>) {
  return (
    <Card>
      <CardContent>
        <Stack spacing={1}>
          <Stack direction="row" justifyContent="space-between" alignItems="center">
            <Typography variant="body2" color="text.secondary">
              {label}
            </Typography>
            {icon}
          </Stack>
          <Typography variant="h5">{value}</Typography>
          {helper ? (
            <Typography variant="caption" color="text.secondary">
              {helper}
            </Typography>
          ) : null}
        </Stack>
      </CardContent>
    </Card>
  );
}
