import { api } from "@/lib/axios";
import type { ChangePasswordPayload } from "../types";

export async function changePassword(
  data: ChangePasswordPayload
) {
  const response = await api.post(
    "/auth/change-password",
    data
  );

  return response.data;
}