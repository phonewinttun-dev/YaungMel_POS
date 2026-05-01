"use client";

import type { ReactNode } from "react";

interface BadgeProps {
  variant?: "primary" | "success" | "warning" | "danger" | "info" | "default";
  children: ReactNode;
  className?: string;
}

const variantStyles = {
  primary:
    "bg-[var(--accent-primary-soft)] text-[var(--accent-primary)] border-[var(--accent-primary)]",
  success:
    "bg-[var(--accent-success-soft)] text-[var(--accent-success)] border-[var(--accent-success)]",
  warning:
    "bg-[var(--accent-warning-soft)] text-[var(--accent-warning)] border-[var(--accent-warning)]",
  danger:
    "bg-[var(--accent-danger-soft)] text-[var(--accent-danger)] border-[var(--accent-danger)]",
  info: "bg-[var(--accent-info-soft)] text-[var(--accent-info)] border-[var(--accent-info)]",
  default:
    "bg-[var(--bg-tertiary)] text-[var(--text-secondary)] border-[var(--border-primary)]",
};

export function Badge({
  variant = "default",
  children,
  className = "",
}: BadgeProps) {
  return (
    <span
      className={`
        inline-flex items-center px-2.5 py-0.5 rounded-lg text-xs font-medium
        border border-opacity-20 whitespace-nowrap
        ${variantStyles[variant]}
        ${className}
      `}
    >
      {children}
    </span>
  );
}
