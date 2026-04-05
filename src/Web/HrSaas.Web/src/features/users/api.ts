import { http } from "@/lib/http";
import type { UserDto } from "@/types/api";

export const usersApi = {
  list: async () => {
    const { data } = await http.get<UserDto[]>("/users");
    return data;
  },
  getById: async (id: string) => {
    const { data } = await http.get<UserDto>(`/users/${id}`);
    return data;
  },
};
