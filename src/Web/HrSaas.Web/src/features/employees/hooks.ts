import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { qk } from "@/lib/query-keys";
import { useNotify } from "@/hooks/useNotify";
import { employeesApi } from "./api";
import type { CreateEmployeePayload, UpdateEmployeePayload } from "./types";

export function useEmployeeList(page: number, pageSize: number, department?: string) {
  return useQuery({
    queryKey: qk.employees.list(page, pageSize, department),
    queryFn: () => employeesApi.list(page, pageSize, department),
  });
}

export function useEmployeeDetail(id: string) {
  return useQuery({
    queryKey: qk.employees.detail(id),
    queryFn: () => employeesApi.getById(id),
    enabled: !!id,
  });
}

export function useCreateEmployee(onDone: () => void) {
  const qc = useQueryClient();
  const notify = useNotify();

  return useMutation({
    mutationFn: (payload: CreateEmployeePayload) => employeesApi.create(payload),
    onSuccess: () => {
      void qc.invalidateQueries({ queryKey: qk.employees.all });
      onDone();
      notify.success("Employee created");
    },
    onError: notify.error,
  });
}

export function useUpdateEmployee(onDone: () => void) {
  const qc = useQueryClient();
  const notify = useNotify();

  return useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: UpdateEmployeePayload }) =>
      employeesApi.update(id, payload),
    onSuccess: () => {
      void qc.invalidateQueries({ queryKey: qk.employees.all });
      onDone();
      notify.success("Employee updated");
    },
    onError: notify.error,
  });
}

export function useDeleteEmployee() {
  const qc = useQueryClient();
  const notify = useNotify();

  return useMutation({
    mutationFn: employeesApi.delete,
    onSuccess: () => {
      void qc.invalidateQueries({ queryKey: qk.employees.all });
      notify.success("Employee deleted");
    },
    onError: notify.error,
  });
}
