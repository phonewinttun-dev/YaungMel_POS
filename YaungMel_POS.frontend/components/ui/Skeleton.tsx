"use client";

export function SkeletonCard() {
  return (
    <div className="rounded-2xl border border-[var(--border-primary)] bg-[var(--bg-card)] p-5">
      <div className="flex items-start justify-between">
        <div className="space-y-3 flex-1">
          <div className="h-3 w-24 rounded-full bg-[var(--bg-tertiary)] animate-[shimmer_1.5s_infinite] bg-[length:200%_100%] bg-gradient-to-r from-[var(--bg-tertiary)] via-[var(--bg-hover)] to-[var(--bg-tertiary)]" />
          <div className="h-6 w-16 rounded-full bg-[var(--bg-tertiary)] animate-[shimmer_1.5s_infinite] bg-[length:200%_100%] bg-gradient-to-r from-[var(--bg-tertiary)] via-[var(--bg-hover)] to-[var(--bg-tertiary)]" />
        </div>
        <div className="w-11 h-11 rounded-xl bg-[var(--bg-tertiary)] animate-[shimmer_1.5s_infinite] bg-[length:200%_100%] bg-gradient-to-r from-[var(--bg-tertiary)] via-[var(--bg-hover)] to-[var(--bg-tertiary)]" />
      </div>
    </div>
  );
}

export function SkeletonTable({ rows = 5 }: { rows?: number }) {
  return (
    <div className="space-y-3">
      {Array.from({ length: rows }).map((_, i) => (
        <div
          key={i}
          className="h-12 rounded-xl bg-[var(--bg-tertiary)] animate-[shimmer_1.5s_infinite] bg-[length:200%_100%] bg-gradient-to-r from-[var(--bg-tertiary)] via-[var(--bg-hover)] to-[var(--bg-tertiary)]"
          style={{ animationDelay: `${i * 100}ms` }}
        />
      ))}
    </div>
  );
}
