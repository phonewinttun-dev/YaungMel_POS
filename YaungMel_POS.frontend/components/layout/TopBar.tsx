"use client";

import { Badge } from "@/components/ui/Badge";
import { ThemeToggle } from "@/components/ui/ThemeToggle";
import { useAuth } from "@/lib/auth-context";
import { LogOut, Menu, PlusCircle } from "lucide-react";
import { useRouter } from "next/navigation";

interface TopBarProps {
  onMenuClick: () => void;
  sidebarCollapsed: boolean;
}

export function TopBar({ onMenuClick }: TopBarProps) {
  const { user, logout } = useAuth();
  const router = useRouter();

  const handleLogout = () => {
    logout();
    router.push("/login");
  };

  const handleShortcutMenu = () => {
    // Logic to open shortcut menu modal
    console.log("Open shortcut menu");
  };

  const roleBadgeVariant =
    user?.role === "Admin"
      ? "primary"
      : user?.role === "Staff"
      ? "info"
      : "success";

  return (
    <header className="h-16 border-b border-[var(--border-primary)] bg-[var(--bg-card)] backdrop-blur-sm flex items-center justify-between px-4 md:px-6 shrink-0">
      {/* Left: Mobile menu + Title */}
      <div className="flex items-center gap-3">
        <button
          onClick={onMenuClick}
          className="lg:hidden p-2 rounded-xl text-[var(--text-secondary)] hover:bg-[var(--bg-hover)] transition-colors cursor-pointer"
        >
          <Menu size={20} />
        </button>
      </div>

      {/* Right: User info + Theme + Logout */}
      <div className="flex items-center gap-3">
        <button
          onClick={handleShortcutMenu}
          className="p-2 rounded-xl text-[var(--text-secondary)] hover:bg-[var(--bg-hover)] transition-colors cursor-pointer"
          title="Shortcut Menu"
        >
          <PlusCircle size={20} />
        </button>

        <ThemeToggle />

        {user && (
          <div className="flex items-center gap-3 pl-3 border-l border-[var(--border-primary)]">
            <div className="hidden sm:block text-right">
              <p className="text-sm font-medium text-[var(--text-primary)]">
                {user.username || user.mobileNum}
              </p>
              <Badge variant={roleBadgeVariant as "primary" | "info" | "success"}>
                {user.role}
              </Badge>
            </div>

            <button
              onClick={handleLogout}
              className="p-2.5 rounded-xl text-[var(--text-tertiary)] hover:text-[var(--accent-danger)] hover:bg-[var(--accent-danger-soft)] transition-colors cursor-pointer"
              title="Logout"
            >
              <LogOut size={18} />
            </button>
          </div>
        )}
      </div>
    </header>
  );
}
