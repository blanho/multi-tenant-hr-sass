import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { api } from "@/lib/api";
import { qk } from "@/lib/query-keys";
import { useNotify } from "@/components/feedback/useNotify";
import type { ApplyLeavePayload } from "@/types/api";

export function usePendingLeave() {
  return useQuery({
    queryKey: qk.leave.pending,
    queryFn: api.leave.getPending,
  });
}

export function useLeaveBalance(employeeId: string | null, year: number) {
  return useQuery({
    queryKey: qk.leave.balance(employeeId ?? "", year),
    queryFn: () => api.leave.getBalance(employeeId ?? "", year),
    enabled: !!employeeId,
  });
}

export function useLeaveHistory(employeeId: string | null, year: number) {
  return useQuery({
    queryKey: qk.leave.byEmployee(employeeId ?? "", year),
    queryFn: () => api.leave.getByEmployee(employeeId ?? "", year),
    enabled: !!employeeId,
  });
}

export function useApplyLeave(onDone: () => void) {
  const qc = useQueryClient();
  const notify = useNotify();

  return useMutation({
    mutationFn: (payload: ApplyLeavePayload) => api.leave.apply(payload),
    onSuccess: () => {
      void qc.invalidateQueries({ queryKey: qk.leave.all });
      onDone();
      notify.success("Leave request submitted");
    },
    onError: notify.error,
  });
}

export function useApproveLeave() {
  const qc = useQueryClient();
  const notify = useNotify();

  return useMutation({
    mutationFn: api.leave.approve,
    onSuccess: () => {
      void qc.invalidateQueries({ queryKey: qk.leave.all });
      notify.success("Leave approved");
    },
    onError: notify.error,
  });
}

export function useRejectLeave(onDone: () => void) {
  const qc = useQueryClient();
  const notify = useNotify();

  return useMutation({
    mutationFn: ({ id, note }: { id: string; note: string }) =>
      api.leave.reject(id, note),
    onSuccess: () => {
      void qc.invalidateQueries({ queryKey: qk.leave.all });
      onDone();
      notify.success("Leave rejected");
    },
    onError: notify.error,
  });
}

export function useCancelLeave() {
  const qc = useQueryClient();
  const notify = useNotify();

  return useMutation({
    mutationFn: api.leave.cancel,
    onSuccess: () => {
      void qc.invalidateQueries({ queryKey: qk.leave.all });
      notify.success("Leave cancelled");
    },
    onError: notify.error,
  });
}
