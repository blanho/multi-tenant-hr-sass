export interface RoleDto {
  id: string;
  tenantId: string;
  name: string;
  isSystem: boolean;
  permissions: string[];
  createdAt: string;
}

export interface CreateRolePayload {
  name: string;
  permissions?: string[];
}

export interface UpdateRolePermissionsPayload {
  permissions: string[];
}

export interface AssignRolePayload {
  userId: string;
  roleId: string;
}
