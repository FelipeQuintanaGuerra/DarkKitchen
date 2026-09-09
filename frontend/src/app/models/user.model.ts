export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  user: UserSummary;
}

export interface RegisterUserRequest {
  name: string;
  surname: string;
  email: string;
  phone: string;
  password: string;
}

export interface UpdateUserRequest {
  name: string;
  surname: string;
  email: string;
  phone: string;
  password: string;
  role: number;
}

export interface UserSummary {
  id: string;
  name: string;
  surname: string;
  email: string;
  phone: string;
  role: string;
  isActive: boolean;
}
