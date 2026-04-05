import type { LeaveStatus, LeaveType } from "@/types/shared";

export interface LeaveRequestDto {
  id: string;
  tenantId: string;
  employeeId: string;
  type: LeaveType;
  status: LeaveStatus;
  startDate: string;
  endDate: string;
  reason: string;
  rejectionNote: string | null;
  durationDays: number;
  createdAt: string;
}

export interface LeaveBalanceDto {
  id: string;
  employeeId: string;
  year: number;
  annualAllowance: number;
  sickAllowance: number;
  annualUsed: number;
  sickUsed: number;
  annualRemaining: number;
  sickRemaining: number;
}

export interface ApplyLeavePayload {
  tenantId: string;
  employeeId: string;
  type: LeaveType;
  startDate: string;
  endDate: string;
  reason: string;
}
