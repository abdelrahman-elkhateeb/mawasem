import { api } from "@/lib/axios";
import type { LoginData, LoginResponse } from "../types";

export async function login(data: LoginData): Promise<LoginResponse> {
  const response = await api.post("/auth/login", data);

  return response.data;
}