import { Stack, Typography } from "@mui/material";

interface DetailRowProps {
  label: string;
  value: string | number;
  maxWidth?: number;
}

export function DetailRow({ label, value, maxWidth = 220 }: Readonly<DetailRowProps>) {
  return (
    <Stack direction="row" justifyContent="space-between" py={0.5}>
      <Typography variant="body2" color="text.secondary" sx={{ minWidth: 100 }}>
        {label}
      </Typography>
      <Typography
        variant="body2"
        fontWeight={500}
        sx={{ maxWidth, wordBreak: "break-all", textAlign: "right" }}
      >
        {value}
      </Typography>
    </Stack>
  );
}
