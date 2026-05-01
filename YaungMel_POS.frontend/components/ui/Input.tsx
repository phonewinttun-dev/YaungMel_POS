"use client";

import type { InputHTMLAttributes, ReactNode } from "react";

interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  label?: string;
  error?: string;
  icon?: ReactNode;
}

export function Input({
  label,
  error,
  icon,
  className = "",
  id,
  ...props
}: InputProps) {
  return (
    <div className="w-full">
      {label && (
        <label
          htmlFor={id}
          className="block text-sm font-medium text-[var(--text-secondary)] mb-1.5"
        >
          {label}
        </label>
      )}
      <div className="relative">
        {icon && (
          <div className="absolute left-3 top-1/2 -translate-y-1/2 text-[var(--text-tertiary)]">
            {icon}
          </div>
        )}
        <input
          id={id}
          className={`
            w-full px-4 py-2.5 text-sm rounded-xl
            bg-[var(--bg-input)] border border-[var(--border-primary)]
            text-[var(--text-primary)] placeholder:text-[var(--text-tertiary)]
            focus:outline-none focus:ring-2 focus:ring-[var(--accent-primary)] focus:border-transparent
            transition-all duration-200
            ${icon ? "pl-10" : ""}
            ${error ? "border-[var(--accent-danger)] focus:ring-[var(--accent-danger)]" : ""}
            ${className}
          `}
          {...props}
        />
      </div>
      {error && (
        <p className="mt-1 text-xs text-[var(--accent-danger)]">{error}</p>
      )}
    </div>
  );
}
