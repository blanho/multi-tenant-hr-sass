import { Box, Stack, Typography } from "@mui/material";

interface JsonBlockProps {
  title: string;
  json: string;
}

export function JsonBlock({ title, json }: Readonly<JsonBlockProps>) {
  return (
    <Stack spacing={0.5}>
      <Typography variant="subtitle2">{title}</Typography>
      <Box
        sx={{
          p: 1.5,
          borderRadius: 1,
          bgcolor: "grey.50",
          fontFamily: "monospace",
          fontSize: 12,
          whiteSpace: "pre-wrap",
          wordBreak: "break-all",
          maxHeight: 200,
          overflow: "auto",
        }}
      >
        {json}
      </Box>
    </Stack>
  );
}
