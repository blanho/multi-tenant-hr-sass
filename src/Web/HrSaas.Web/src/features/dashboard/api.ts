import { http } from "@/lib/http";
import type { FeatureDto } from "@/types/shared";

export const featuresApi = {
  list: async () => {
    const { data } = await http.get<FeatureDto[]>("/features");
    return data;
  },
  check: async (name: string) => {
    const { data } = await http.get<FeatureDto>(`/features/${name}`);
    return data;
  },
};
