"use client";

import type { SelectHTMLAttributes } from "react";

interface SelectProps extends SelectHTMLAttributes<HTMLSelectElement> {
  label?: string;
  options: { value: string | number; label: string }[];
  error?: string;
}

export function Select({
  label,
  options,
  error,
  className = "",
  id,
  ...props
}: SelectProps) {
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
      <select
        id={id}
        className={`
          w-full px-4 py-2.5 text-sm rounded-xl
          bg-[var(--bg-input)] border border-[var(--border-primary)]
          text-[var(--text-primary)]
          focus:outline-none focus:ring-2 focus:ring-[var(--accent-primary)] focus:border-transparent
          transition-all duration-200 cursor-pointer
          ${error ? "border-[var(--accent-danger)]" : ""}
          ${className}
        `}
        {...props}
      >
        {options.map((opt) => (
          <option key={opt.value} value={opt.value}>
            {opt.label}
          </option>
        ))}
      </select>
      {error && (
        <p className="mt-1 text-xs text-[var(--accent-danger)]">{error}</p>
      )}
    </div>
  );
}
