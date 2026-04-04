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
import { Link as RouterLink, useNavigate } from "react-router-dom";
import { api } from "../../lib/api";
import { setSession } from "../../lib/session";
import { extractErrorMessage } from "../../lib/http";

const schema = z.object({
  tenantId: z
    .string()
    .regex(
      /^[0-9a-f]{8}-[0-9a-f]{4}-[1-8][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i,
      "Tenant ID must be a valid GUID",
    ),
  fullName: z.string().min(2, "Full name is required").max(64),
  email: z.email("Valid email is required"),
  password: z.string().min(8, "Minimum 8 characters").max(128),
  confirmPassword: z.string(),
}).refine((data) => data.password === data.confirmPassword, {
  message: "Passwords do not match",
  path: ["confirmPassword"],
});

type RegisterForm = z.infer<typeof schema>;

export function RegisterPage() {
  const navigate = useNavigate();

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<RegisterForm>({
    resolver: zodResolver(schema),
    defaultValues: { tenantId: "", fullName: "", email: "", password: "", confirmPassword: "" },
  });

  const mutation = useMutation({
    mutationFn: (values: RegisterForm) =>
      api.auth.register({
        tenantId: values.tenantId,
        email: values.email,
        password: values.password,
        fullName: values.fullName,
      }),
    onSuccess: (token) => {
      setSession(token);
      navigate("/", { replace: true });
    },
  });

  return (
    <Box sx={{ minHeight: "100vh", display: "grid", placeItems: "center", px: 2 }}>
      <Card sx={{ width: "100%", maxWidth: 480 }}>
        <CardContent sx={{ p: 4 }}>
          <Stack spacing={2.5}>
            <Typography variant="h5">Create Account</Typography>
            <Typography variant="body2" color="text.secondary">
              Register a new user within your tenant
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
              label="Full Name"
              error={!!errors.fullName}
              helperText={errors.fullName?.message}
              {...register("fullName")}
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
            <TextField
              label="Confirm Password"
              type="password"
              error={!!errors.confirmPassword}
              helperText={errors.confirmPassword?.message}
              {...register("confirmPassword")}
            />

            <Button
              variant="contained"
              size="large"
              onClick={handleSubmit((v) => mutation.mutate(v))}
              disabled={mutation.isPending}
            >
              {mutation.isPending ? "Creating account..." : "Register"}
            </Button>

            <Typography variant="body2" textAlign="center" color="text.secondary">
              Already have an account?{" "}
              <Typography
                component={RouterLink}
                to="/login"
                variant="body2"
                color="primary"
                sx={{ fontWeight: 600 }}
              >
                Sign In
              </Typography>
            </Typography>
          </Stack>
        </CardContent>
      </Card>
    </Box>
  );
}
