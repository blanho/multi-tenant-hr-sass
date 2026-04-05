export interface EmployeeSummaryDto {
  id: string;
  name: string;
  department: string;
  position: string;
}

export interface EmployeeDto extends EmployeeSummaryDto {
  tenantId: string;
  email: string;
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateEmployeePayload {
  name: string;
  department: string;
  position: string;
  email: string;
}

export interface UpdateEmployeePayload {
  name: string;
  department: string;
  position: string;
}
