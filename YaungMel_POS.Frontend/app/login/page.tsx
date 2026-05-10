"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/lib/auth-context";
import { useTheme } from "@/lib/theme-context";
import { Input } from "@/components/ui/Input";
import { Button } from "@/components/ui/Button";
import { ThemeToggle } from "@/components/ui/ThemeToggle";
import { toast } from "@/components/ui/Toast";
import { motion } from "framer-motion";
import { Phone, Lock, ArrowRight } from "lucide-react";
import { Logo } from "@/components/ui/Logo";

export default function LoginPage() {
  const router = useRouter();
  const { login } = useAuth();
  const { theme } = useTheme();
  const [mobileNum, setMobileNum] = useState("");
  const [password, setPassword] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [errors, setErrors] = useState<{ mobileNum?: string; password?: string }>({});

  const validate = () => {
    const newErrors: { mobileNum?: string; password?: string } = {};
    if (!mobileNum.trim()) newErrors.mobileNum = "Mobile number is required";
    if (!password) newErrors.password = "Password is required";
    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!validate()) return;

    setIsLoading(true);
    try {
      const result = await login({ mobileNum, password });
      if (result.success) {
        toast("success", "Welcome back! Redirecting...");
        setTimeout(() => router.push("/dashboard"), 500);
      } else {
        toast("error", result.message);
      }
    } catch {
      toast("error", "Something went wrong. Please try again.");
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center relative overflow-hidden">
      {/* Animated Background */}
      <div className="absolute inset-0">
        <div
          className={`absolute inset-0 ${
            theme === "dark"
              ? "bg-gradient-to-br from-[#0c0f1a] via-[#111827] to-[#0c0f1a]"
              : "bg-gradient-to-br from-blue-50 via-white to-violet-50"
          }`}
        />
        {/* Floating orbs */}
        <div
          className={`absolute top-1/4 -left-20 w-72 h-72 rounded-full blur-3xl opacity-20 ${
            theme === "dark" ? "bg-blue-600" : "bg-blue-400"
          }`}
          style={{ animation: "pulse-glow 4s ease-in-out infinite" }}
        />
        <div
          className={`absolute bottom-1/4 -right-20 w-96 h-96 rounded-full blur-3xl opacity-15 ${
            theme === "dark" ? "bg-violet-600" : "bg-violet-400"
          }`}
          style={{ animation: "pulse-glow 6s ease-in-out infinite 2s" }}
        />
        <div
          className={`absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[600px] h-[600px] rounded-full blur-3xl opacity-10 ${
            theme === "dark" ? "bg-cyan-500" : "bg-cyan-300"
          }`}
        />
      </div>

      {/* Theme toggle */}
      <div className="absolute top-6 right-6 z-10">
        <ThemeToggle />
      </div>

      {/* Login Card */}
      <motion.div
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.5 }}
        className="relative z-10 w-full max-w-md mx-4"
      >
        <div
          className={`
            p-8 rounded-3xl border
            ${
              theme === "dark"
                ? "bg-[rgba(15,20,32,0.8)] border-[rgba(255,255,255,0.08)] shadow-2xl"
                : "bg-[rgba(255,255,255,0.85)] border-[rgba(0,0,0,0.06)] shadow-xl"
            }
            backdrop-blur-xl
          `}
        >
          {/* Logo */}
          <div className="flex flex-col items-center mb-8">
            <motion.div
              initial={{ scale: 0 }}
              animate={{ scale: 1 }}
              transition={{ type: "spring", stiffness: 200, delay: 0.2 }}
              className="w-14 h-14 rounded-2xl flex items-center justify-center mb-4 shadow-lg"
            >
              <Logo className="w-full h-full p-1" />
            </motion.div>
            <h1 className="text-2xl font-bold text-[var(--text-primary)]">
              Yaung Mel
            </h1>
            <p className="text-sm text-[var(--text-secondary)] mt-1">
              Sign in to your dashboard
            </p>
          </div>

          {/* Form */}
          <form onSubmit={handleSubmit} className="space-y-5">
            <Input
              label="Mobile Number"
              type="tel"
              placeholder="09xxxxxxxxx"
              value={mobileNum}
              onChange={(e) => {
                setMobileNum(e.target.value);
                if (errors.mobileNum)
                  setErrors((prev) => ({ ...prev, mobileNum: undefined }));
              }}
              error={errors.mobileNum}
              icon={<Phone size={18} />}
              autoComplete="tel"
              id="login-mobile"
            />

            <Input
              label="Password"
              type="password"
              placeholder="••••••••"
              value={password}
              onChange={(e) => {
                setPassword(e.target.value);
                if (errors.password)
                  setErrors((prev) => ({ ...prev, password: undefined }));
              }}
              error={errors.password}
              icon={<Lock size={18} />}
              autoComplete="current-password"
              id="login-password"
            />

            <Button
              type="submit"
              size="lg"
              isLoading={isLoading}
              className="w-full"
              icon={!isLoading ? <ArrowRight size={18} /> : undefined}
            >
              Sign In
            </Button>
          </form>


        </div>

        {/* Footer */}
        <p className="text-center text-xs text-[var(--text-tertiary)] mt-6">
          Yaung Mel v2.0 — Point of Sale System
        </p>
      </motion.div>
    </div>
  );
}
