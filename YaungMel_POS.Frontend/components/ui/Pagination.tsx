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

  // Generate page numbers to show
  const getPageNumbers = () => {
    const pages = [];
    const showMax = 5;

    if (totalPages <= showMax + 2) {
      for (let i = 1; i <= totalPages; i++) pages.push(i);
    } else {
      if (currentPage <= 3) {
        for (let i = 1; i <= 4; i++) pages.push(i);
        pages.push("...");
        pages.push(totalPages - 1);
        pages.push(totalPages);
      } else if (currentPage >= totalPages - 2) {
        pages.push(1);
        pages.push(2);
        pages.push("...");
        for (let i = totalPages - 3; i <= totalPages; i++) pages.push(i);
      } else {
        pages.push(1);
        pages.push("...");
        for (let i = currentPage - 1; i <= currentPage + 1; i++) pages.push(i);
        pages.push("...");
        pages.push(totalPages);
      }
    }
    return pages;
  };

  return (
    <div className={`flex items-center justify-center gap-1 mt-6 ${className}`}>
      <button
        onClick={() => onPageChange(currentPage - 1)}
        disabled={currentPage === 1}
        className="flex items-center gap-1 px-3 py-2 text-sm font-medium text-[var(--text-tertiary)] hover:text-[var(--text-primary)] disabled:opacity-30 disabled:cursor-not-allowed transition-colors"
      >
        <ChevronLeft size={16} />
        <span>Previous</span>
      </button>

      <div className="flex items-center gap-1 mx-2">
        {getPageNumbers().map((p, idx) => (
          p === "..." ? (
            <span key={`ellipsis-${idx}`} className="px-2 text-[var(--text-tertiary)]">...</span>
          ) : (
            <button
              key={p}
              onClick={() => onPageChange(p as number)}
              className={`min-w-[32px] h-[32px] flex items-center justify-center rounded-lg text-sm font-medium transition-all ${
                currentPage === p
                  ? "bg-[#1a73e8] text-white shadow-sm"
                  : "text-[var(--text-secondary)] hover:bg-[var(--bg-hover)]"
              }`}
            >
              {p}
            </button>
          )
        ))}
      </div>

      <button
        onClick={() => onPageChange(currentPage + 1)}
        disabled={currentPage === totalPages}
        className="flex items-center gap-1 px-3 py-2 text-sm font-medium text-[#1a73e8] hover:text-[#1557b0] disabled:opacity-30 disabled:cursor-not-allowed transition-colors"
      >
        <span>Next</span>
        <ChevronRight size={16} />
      </button>
    </div>
  );
}
