import { useSnackbar } from "notistack";
import { useCallback } from "react";
import { extractErrorMessage } from "@/lib/http";

export function useNotify() {
  const { enqueueSnackbar } = useSnackbar();

  const success = useCallback(
    (message: string) => enqueueSnackbar(message, { variant: "success" }),
    [enqueueSnackbar],
  );

  const error = useCallback(
    (err: unknown) =>
      enqueueSnackbar(extractErrorMessage(err), { variant: "error", persist: false }),
    [enqueueSnackbar],
  );

  const info = useCallback(
    (message: string) => enqueueSnackbar(message, { variant: "info" }),
    [enqueueSnackbar],
  );

  const warn = useCallback(
    (message: string) => enqueueSnackbar(message, { variant: "warning" }),
    [enqueueSnackbar],
  );

  return { success, error, info, warn };
}
