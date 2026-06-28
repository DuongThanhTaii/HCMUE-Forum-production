export interface User {
  id: string;
  email: string;
  fullName: string;
  avatar?: string;
  roles: string[];
  studentId?: string;
}

export interface AuthPayload {
  user: User;
  accessToken: string;
  refreshToken: string;
}

