"use client";

import { useState, useEffect, useCallback, createContext, useContext } from "react";
import { AnimatePresence, motion } from "framer-motion";
import { CheckCircle, XCircle, AlertTriangle, X, Info } from "lucide-react";
import type { ReactNode } from "react";

type ToastType = "success" | "error" | "warning" | "info";

interface ToastItem {
  id: number;
  type: ToastType;
  message: string;
}

interface ToastContextValue {
  addToast: (type: ToastType, message: string) => void;
}

const ToastContext = createContext<ToastContextValue>({
  addToast: () => {},
});

const icons: Record<ToastType, ReactNode> = {
  success: <CheckCircle size={18} />,
  error: <XCircle size={18} />,
  warning: <AlertTriangle size={18} />,
  info: <Info size={18} />,
};

const colors: Record<ToastType, string> = {
  success: "text-[var(--accent-success)] bg-[var(--accent-success-soft)] border-[var(--accent-success)]",
  error: "text-[var(--accent-danger)] bg-[var(--accent-danger-soft)] border-[var(--accent-danger)]",
  warning: "text-[var(--accent-warning)] bg-[var(--accent-warning-soft)] border-[var(--accent-warning)]",
  info: "text-[var(--accent-info)] bg-[var(--accent-info-soft)] border-[var(--accent-info)]",
};

let toastId = 0;
let globalAddToast: ((type: ToastType, message: string) => void) | null = null;

export function toast(type: ToastType, message: string) {
  globalAddToast?.(type, message);
}

export function ToastContainer() {
  const [toasts, setToasts] = useState<ToastItem[]>([]);

  const addToast = useCallback((type: ToastType, message: string) => {
    const id = ++toastId;
    setToasts((prev) => [...prev, { id, type, message }]);
    setTimeout(() => {
      setToasts((prev) => prev.filter((t) => t.id !== id));
    }, 4000);
  }, []);

  useEffect(() => {
    globalAddToast = addToast;
    return () => {
      globalAddToast = null;
    };
  }, [addToast]);

  const removeToast = useCallback((id: number) => {
    setToasts((prev) => prev.filter((t) => t.id !== id));
  }, []);

  return (
    <div className="fixed top-4 right-4 z-[100] flex flex-col gap-2 max-w-sm w-full pointer-events-none">
      <AnimatePresence>
        {toasts.map((t) => (
          <motion.div
            key={t.id}
            initial={{ opacity: 0, x: 100, scale: 0.95 }}
            animate={{ opacity: 1, x: 0, scale: 1 }}
            exit={{ opacity: 0, x: 100, scale: 0.95 }}
            transition={{ duration: 0.25 }}
            className={`
              pointer-events-auto flex items-center gap-3 p-4 rounded-xl
              border backdrop-blur-lg shadow-[var(--shadow-md)]
              ${colors[t.type]}
            `}
          >
            {icons[t.type]}
            <p className="flex-1 text-sm font-medium text-[var(--text-primary)]">
              {t.message}
            </p>
            <button
              onClick={() => removeToast(t.id)}
              className="p-1 rounded-md hover:bg-[var(--bg-hover)] text-[var(--text-tertiary)] transition-colors cursor-pointer"
            >
              <X size={14} />
            </button>
          </motion.div>
        ))}
      </AnimatePresence>
    </div>
  );
}

export function ToastProvider({ children }: { children: ReactNode }) {
  return (
    <>
      {children}
      <ToastContainer />
    </>
  );
}
