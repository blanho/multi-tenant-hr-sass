export interface UserDto {
  id: string;
  tenantId: string;
  email: string;
  roleId: string;
  roleName: string;
  permissions: string[];
  isActive: boolean;
  createdAt: string;
}

export interface AuthTokenDto {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: UserDto;
}

export interface LoginPayload {
  tenantId: string;
  email: string;
  password: string;
}

export interface RegisterPayload {
  tenantId: string;
  email: string;
  password: string;
  fullName?: string;
}
