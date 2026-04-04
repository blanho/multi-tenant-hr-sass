import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Stack,
  TextField,
  Typography,
} from "@mui/material";
import { useMutation } from "@tanstack/react-query";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { useLocation, useNavigate } from "react-router-dom";
import { useAuth } from "./auth-context";
import { extractErrorMessage } from "../../lib/http";

type LoginFormValues = {
  tenantId: string;
  email: string;
  password: string;
};

const schema = z.object({
  tenantId: z
    .string()
    .regex(
      /^[0-9a-f]{8}-[0-9a-f]{4}-[1-8][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i,
      "Tenant ID must be a valid GUID",
    ),
  email: z.email("Email is invalid"),
  password: z.string().min(8, "Password must be at least 8 characters"),
});

export function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginFormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      tenantId: "",
      email: "",
      password: "",
    },
  });

  const mutation = useMutation({
    mutationFn: login,
    onSuccess: () => {
      const next = (location.state as { from?: string } | null)?.from ?? "/";
      navigate(next, { replace: true });
    },
  });

  return (
    <Box
      sx={{
        minHeight: "100vh",
        display: "grid",
        placeItems: "center",
        px: 2,
      }}
    >
      <Card sx={{ width: "100%", maxWidth: 460 }}>
        <CardContent sx={{ p: 4 }}>
          <Stack spacing={2.5}>
            <Typography variant="h5">HrSaas Portal</Typography>
            <Typography variant="body2" color="text.secondary">
              Multi-tenant secure sign-in for HR operations
            </Typography>

            {mutation.isError && (
              <Alert severity="error">{extractErrorMessage(mutation.error)}</Alert>
            )}

            <TextField
              label="Tenant ID"
              placeholder="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
              error={!!errors.tenantId}
              helperText={errors.tenantId?.message}
              {...register("tenantId")}
            />
            <TextField
              label="Email"
              type="email"
              error={!!errors.email}
              helperText={errors.email?.message}
              {...register("email")}
            />
            <TextField
              label="Password"
              type="password"
              error={!!errors.password}
              helperText={errors.password?.message}
              {...register("password")}
            />

            <Button
              variant="contained"
              size="large"
              onClick={handleSubmit((values) => mutation.mutate(values))}
              disabled={mutation.isPending}
            >
              {mutation.isPending ? "Signing in..." : "Sign In"}
            </Button>
          </Stack>
        </CardContent>
      </Card>
    </Box>
  );
}
