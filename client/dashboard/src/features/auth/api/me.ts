import { api } from "@/lib/axios";
import type { User } from "../types";

export async function getMe(): Promise<User> {
  const response = await api.get("/auth/me");

  return response.data;
}