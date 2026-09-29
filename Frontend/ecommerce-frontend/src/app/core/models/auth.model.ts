// شكل البيانات اللي بنبعتها لما نعمل تسجيل حساب جديد
export interface RegisterRequest {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  phoneNumber: string;
  role: 'Customer' | 'Seller';
  address?: string;      // مطلوبة بس لو Customer
  storeName?: string;    // مطلوبة بس لو Seller
}

// شكل البيانات اللي بنبعتها لما نعمل تسجيل دخول
export interface LoginRequest {
  email: string;
  password: string;
}

// شكل الرد اللي بيرجعلنا من الـ Backend بعد login/register
export interface AuthResponse {
  success: boolean;
  message: string;
  token: string | null;
  expiresAt: string | null;
  refreshToken: string | null;
}
export interface ForgotPasswordRequest {
  email: string;
}

export interface ResetPasswordRequest {
  email: string;
  token: string;
  newPassword: string;
}