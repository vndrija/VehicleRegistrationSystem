export interface LoginRequest {
  username: string;
  password: string;
}

export interface RegisterRequest {
  username: string;
  password: string;
  email: string;
  role: string;
}

export interface LoginResponse {
  token: string;
  refreshToken: string;
  expiresAt: Date;
  user: UserDto;
}

export interface UserDto {
  id: number;
  username: string;
  email: string;
  role: string;
}

export interface AuthResponse {
  message: string;
  data: LoginResponse;
}

export interface ErrorResponse {
  message: string;
  errors: string[];
}
