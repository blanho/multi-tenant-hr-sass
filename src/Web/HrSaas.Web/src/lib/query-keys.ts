export const qk = {
  auth: {
    me: ["auth", "me"] as const,
  },
  users: {
    all: ["users"] as const,
    detail: (id: string) => ["users", id] as const,
  },
  roles: {
    all: ["roles"] as const,
    detail: (id: string) => ["roles", id] as const,
    permissions: (id: string) => ["roles", id, "permissions"] as const,
  },
  employees: {
    all: ["employees"] as const,
    list: (page: number, pageSize: number, department?: string) =>
      ["employees", "list", { page, pageSize, department }] as const,
    detail: (id: string) => ["employees", id] as const,
  },
  leave: {
    all: ["leave"] as const,
    pending: ["leave", "pending"] as const,
    byEmployee: (id: string, year?: number) =>
      ["leave", "employee", id, { year }] as const,
    balance: (id: string, year?: number) =>
      ["leave", "balance", id, { year }] as const,
  },
  tenants: {
    all: ["tenants"] as const,
    detail: (id: string) => ["tenants", id] as const,
  },
  billing: {
    subscription: ["billing", "subscription"] as const,
  },
  notifications: {
    all: ["notifications"] as const,
    list: (params: Record<string, unknown>) =>
      ["notifications", "list", params] as const,
    detail: (id: string) => ["notifications", id] as const,
    stats: ["notifications", "stats"] as const,
    preferences: ["notifications", "preferences"] as const,
    templates: ["notifications", "templates"] as const,
  },
  features: {
    all: ["features"] as const,
    check: (name: string) => ["features", name] as const,
  },
  files: {
    all: ["files"] as const,
    list: (params: Record<string, unknown>) => ["files", "list", params] as const,
    detail: (id: string) => ["files", id] as const,
  },
  auditLogs: {
    all: ["auditLogs"] as const,
    list: (params: Record<string, unknown>) => ["auditLogs", "list", params] as const,
    detail: (id: string) => ["auditLogs", id] as const,
  },
};
