"use client";

import {
  createContext,
  useContext,
  useEffect,
  useState,
  useCallback,
  type ReactNode,
} from "react";
import { authApi } from "./api";
import type { LoginRequest, RegisterRequest, UserRole } from "./types";

function generateUsername(mobileNum: string): string {
  const digits = mobileNum.replace(/\D/g, "");
  const last8 = digits.slice(-8);
  return `YM-${last8}`;
}

interface User {
  mobileNum: string;
  role: string;
  token: string;
  username: string;
}

interface AuthContextValue {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  isAdmin: boolean;
  isStaff: boolean;
  isCustomer: boolean;
  login: (data: LoginRequest) => Promise<{ success: boolean; message: string }>;
  register: (data: RegisterRequest) => Promise<{ success: boolean; message: string }>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextValue>({
  user: null,
  isAuthenticated: false,
  isLoading: true,
  isAdmin: false,
  isStaff: false,
  isCustomer: false,
  login: async () => ({ success: false, message: "" }),
  register: async () => ({ success: false, message: "" }),
  logout: () => {},
});

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const stored = localStorage.getItem("user");
    const token = localStorage.getItem("accessToken");
    if (stored && token) {
      try {
        const parsed = JSON.parse(stored);
        setUser({ ...parsed, token, username: parsed.username || generateUsername(parsed.mobileNum) });
      } catch {
        localStorage.removeItem("user");
        localStorage.removeItem("accessToken");
      }
    }
    setIsLoading(false);
  }, []);

  const login = useCallback(async (data: LoginRequest) => {
    try {
      const res = await authApi.login(data);
      if (res.isSuccess && res.data) {
        const username = generateUsername(res.data.mobileNum);
        const u: User = {
          mobileNum: res.data.mobileNum,
          role: res.data.role,
          token: res.data.accessToken,
          username,
        };
        localStorage.setItem("accessToken", res.data.accessToken);
        localStorage.setItem("user", JSON.stringify({ mobileNum: u.mobileNum, role: u.role, username: u.username }));
        setUser(u);
        return { success: true, message: "Login successful" };
      }
      return { success: false, message: res.message || "Login failed" };
    } catch (err: unknown) {
      const message =
        (err as { response?: { data?: { message?: string } } })?.response?.data?.message ||
        "Login failed. Please try again.";
      return { success: false, message };
    }
  }, []);

  const register = useCallback(async (data: RegisterRequest) => {
    try {
      const res = await authApi.register(data);
      if (res.isSuccess) {
        return { success: true, message: "Account created successfully" };
      }
      return { success: false, message: res.message || "Registration failed" };
    } catch (err: unknown) {
      const message =
        (err as { response?: { data?: { message?: string } } })?.response?.data?.message ||
        "Registration failed. Please try again.";
      return { success: false, message };
    }
  }, []);

  const logout = useCallback(() => {
    localStorage.removeItem("accessToken");
    localStorage.removeItem("user");
    setUser(null);
  }, []);

  const isAuthenticated = !!user;
  const isAdmin = user?.role === "Admin";
  const isStaff = user?.role === "Staff";
  const isCustomer = user?.role === "Customer";

  return (
    <AuthContext.Provider
      value={{
        user,
        isAuthenticated,
        isLoading,
        isAdmin,
        isStaff,
        isCustomer,
        login,
        register,
        logout,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export const useAuth = () => useContext(AuthContext);
