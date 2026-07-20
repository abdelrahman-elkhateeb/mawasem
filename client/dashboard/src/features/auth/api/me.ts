import { api } from "@/lib/axios";
import type { LoginResponse } from "../types";

export async function getMe(): Promise<LoginResponse> {
  const response = await api.get("/auth/me");

  console.log(response.data);


  return response.data;
}