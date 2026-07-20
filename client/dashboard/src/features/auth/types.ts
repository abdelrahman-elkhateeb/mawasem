
export interface User {
  id: number;
  fullNameAr: string;
  fullNameEn: string;
  email: string;
  mustChangePassword: boolean;
  roles: string[];
  permissions: string[];
}

export interface LoginResponse {
  user: User;
}

export interface LoginData {
  email: string;
  password: string;
}

export interface ChangePasswordPayload {
  currentPassword: string;
  newPassword: string;
  confirmNewPassword: string;
}