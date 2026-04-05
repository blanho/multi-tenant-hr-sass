import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { qk } from "@/lib/query-keys";
import { useNotify } from "@/hooks/useNotify";
import { leaveApi } from "./api";
import type { ApplyLeavePayload } from "./types";

export function usePendingLeave() {
  return useQuery({
    queryKey: qk.leave.pending,
    queryFn: leaveApi.getPending,
  });
}

export function useLeaveBalance(employeeId: string | null, year: number) {
  return useQuery({
    queryKey: qk.leave.balance(employeeId ?? "", year),
    queryFn: () => leaveApi.getBalance(employeeId ?? "", year),
    enabled: !!employeeId,
  });
}

export function useLeaveHistory(employeeId: string | null, year: number) {
  return useQuery({
    queryKey: qk.leave.byEmployee(employeeId ?? "", year),
    queryFn: () => leaveApi.getByEmployee(employeeId ?? "", year),
    enabled: !!employeeId,
  });
}

export function useApplyLeave(onDone: () => void) {
  const qc = useQueryClient();
  const notify = useNotify();

  return useMutation({
    mutationFn: (payload: ApplyLeavePayload) => leaveApi.apply(payload),
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
    mutationFn: leaveApi.approve,
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
      leaveApi.reject(id, note),
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
    mutationFn: leaveApi.cancel,
    onSuccess: () => {
      void qc.invalidateQueries({ queryKey: qk.leave.all });
      notify.success("Leave cancelled");
    },
    onError: notify.error,
  });
}
