import React from "react";
import { ChevronLeft, ChevronRight, ChevronsLeft, ChevronsRight } from "lucide-react";

interface PaginationProps {
  currentPage: number;
  totalPages: number;
  onPageChange: (page: number) => void;
  className?: string;
}

export function Pagination({
  currentPage,
  totalPages,
  onPageChange,
  className = "",
}: PaginationProps) {
  if (totalPages <= 1) return null;

  const pages = [];
  const maxVisible = 5;
  
  let start = Math.max(1, currentPage - Math.floor(maxVisible / 2));
    const end = Math.min(totalPages, start + maxVisible - 1);
  
  if (end - start + 1 < maxVisible) {
    start = Math.max(1, end - maxVisible + 1);
  }

  for (let i = start; i <= end; i++) {
    pages.push(i);
  }

  return (
    <div className={`flex items-center justify-center gap-1.5 mt-6 ${className}`}>
      <button
        onClick={() => onPageChange(1)}
        disabled={currentPage === 1}
        className="p-2 rounded-lg border border-[var(--border-primary)] text-[var(--text-tertiary)] hover:bg-[var(--bg-hover)] disabled:opacity-30 disabled:hover:bg-transparent transition-colors"
      >
        <ChevronsLeft size={16} />
      </button>
      <button
        onClick={() => onPageChange(currentPage - 1)}
        disabled={currentPage === 1}
        className="p-2 rounded-lg border border-[var(--border-primary)] text-[var(--text-tertiary)] hover:bg-[var(--bg-hover)] disabled:opacity-30 disabled:hover:bg-transparent transition-colors"
      >
        <ChevronLeft size={16} />
      </button>

      {start > 1 && <span className="px-2 text-[var(--text-tertiary)]">...</span>}

      {pages.map((p) => (
        <button
          key={p}
          onClick={() => onPageChange(p)}
          className={`min-w-[36px] h-[36px] flex items-center justify-center rounded-lg text-sm font-medium transition-all ${
            currentPage === p
              ? "bg-[var(--accent-primary)] text-white shadow-sm"
              : "border border-[var(--border-primary)] text-[var(--text-secondary)] hover:bg-[var(--bg-hover)]"
          }`}
        >
          {p}
        </button>
      ))}

      {end < totalPages && <span className="px-2 text-[var(--text-tertiary)]">...</span>}

      <button
        onClick={() => onPageChange(currentPage + 1)}
        disabled={currentPage === totalPages}
        className="p-2 rounded-lg border border-[var(--border-primary)] text-[var(--text-tertiary)] hover:bg-[var(--bg-hover)] disabled:opacity-30 disabled:hover:bg-transparent transition-colors"
      >
        <ChevronRight size={16} />
      </button>
      <button
        onClick={() => onPageChange(totalPages)}
        disabled={currentPage === totalPages}
        className="p-2 rounded-lg border border-[var(--border-primary)] text-[var(--text-tertiary)] hover:bg-[var(--bg-hover)] disabled:opacity-30 disabled:hover:bg-transparent transition-colors"
      >
        <ChevronsRight size={16} />
      </button>
    </div>
  );
}
