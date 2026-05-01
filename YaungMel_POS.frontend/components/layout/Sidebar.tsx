"use client";

import { useAuth } from "@/lib/auth-context";
import { AnimatePresence, motion } from "framer-motion";
import {
  ChevronLeft,
  ClipboardList,
  Gift,
  LayoutDashboard,
  Package,
  Receipt,
  Search,
  ShoppingBag,
  ShoppingCart,
  Star,
  Tags,
  Trophy,
  Users,
  Warehouse,
  X,
} from "lucide-react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import type { ReactNode } from "react";
import { useState } from "react";

interface NavItem {
  label: string;
  href: string;
  icon: ReactNode;
  roles: string[];
}

const navItems: NavItem[] = [
  {
    label: "Dashboard",
    href: "/dashboard",
    icon: <LayoutDashboard size={20} />,
    roles: ["Admin"],
  },
  {
    label: "POS",
    href: "/pos",
    icon: <ShoppingCart size={20} />,
    roles: ["Admin", "Staff"],
  },
  {
    label: "Products",
    href: "/products",
    icon: <Package size={20} />,
    roles: ["Admin"],
  },
  {
    label: "Categories",
    href: "/categories",
    icon: <Tags size={20} />,
    roles: ["Admin"],
  },
  {
    label: "Sales",
    href: "/sales",
    icon: <Receipt size={20} />,
    roles: ["Admin", "Staff"],
  },
  {
    label: "Inventory",
    href: "/inventory",
    icon: <Warehouse size={20} />,
    roles: ["Admin", "Staff"],
  },
  {
    label: "Loyalty",
    href: "/loyalty",
    icon: <Gift size={20} />,
    roles: ["Admin", "Staff"],
  },
  {
    label: "Rewards",
    href: "/rewards",
    icon: <Trophy size={20} />,
    roles: ["Admin"],
  },
  {
    label: "Redemptions",
    href: "/redemptions",
    icon: <ClipboardList size={20} />,
    roles: ["Admin", "Staff"],
  },
  {
    label: "Search",
    href: "/search",
    icon: <Search size={20} />,
    roles: ["Admin"],
  },
  {
    label: "Users",
    href: "/users",
    icon: <Users size={20} />,
    roles: ["Admin", "Staff"],
  },
  {
    label: "My Points",
    href: "/my-points",
    icon: <Star size={20} />,
    roles: ["Customer"],
  },
];

interface FavoritesItem {
  label: string;
  href: string;
  icon: ReactNode;
}

const favorites: FavoritesItem[] = [
  {
    label: "Reports",
    href: "/reports",
    icon: <ClipboardList size={20} />,
  },
];

interface SidebarProps {
  collapsed: boolean;
  onToggle: () => void;
  mobileOpen: boolean;
  onMobileClose: () => void;
}

export function Sidebar({
  collapsed,
  onToggle,
  mobileOpen,
  onMobileClose,
}: SidebarProps) {
  const pathname = usePathname();
  const { user } = useAuth();
  const userRole = user?.role || "";

  const [favoriteItems, setFavoriteItems] = useState<FavoritesItem[]>(favorites);

  const filteredItems = navItems.filter((item) =>
    item.roles.includes(userRole)
  );

  const sidebarContent = (
    <div className="flex flex-col h-full">
      {/* Logo */}
      <div className="flex items-center gap-3 px-4 py-5 border-b border-[var(--border-primary)]">
        <div className="w-10 h-10 rounded-xl gradient-primary flex items-center justify-center shadow-lg shrink-0">
          <ShoppingBag className="text-white" size={22} />
        </div>
        <AnimatePresence>
          {!collapsed && (
            <motion.div
              initial={{ opacity: 0, width: 0 }}
              animate={{ opacity: 1, width: "auto" }}
              exit={{ opacity: 0, width: 0 }}
              className="overflow-hidden whitespace-nowrap"
            >
              <h1 className="text-lg font-bold text-[var(--text-primary)]">
                MiniPOS
              </h1>
              <p className="text-[10px] text-[var(--text-tertiary)] -mt-0.5">
                Point of Sale
              </p>
            </motion.div>
          )}
        </AnimatePresence>
      </div>

      {/* Favorites Section */}
      <div className="px-3 py-4 border-b border-[var(--border-primary)]">
        <h3 className="text-xs font-semibold text-[var(--text-secondary)] uppercase tracking-wider mb-2">
          Favorites
        </h3>
        {favoriteItems.map((fav) => (
          <Link
            key={fav.href}
            href={fav.href}
            onClick={onMobileClose}
            className={`
              flex items-center gap-3 px-3 py-2.5 rounded-xl text-sm font-medium
              transition-all duration-200 group relative
              text-[var(--text-secondary)] hover:bg-[var(--bg-hover)] hover:text-[var(--text-primary)]
            `}
          >
            <span className="shrink-0">{fav.icon}</span>
            <AnimatePresence>
              {!collapsed && (
                <motion.span
                  initial={{ opacity: 0, width: 0 }}
                  animate={{ opacity: 1, width: "auto" }}
                  exit={{ opacity: 0, width: 0 }}
                  className="overflow-hidden whitespace-nowrap"
                >
                  {fav.label}
                </motion.span>
              )}
            </AnimatePresence>
          </Link>
        ))}
      </div>

      {/* Navigation */}
      <nav className="flex-1 py-4 px-3 space-y-1 overflow-y-auto">
        {filteredItems.map((item) => {
          const isActive = pathname === item.href;
          return (
            <Link
              key={item.href}
              href={item.href}
              onClick={onMobileClose}
              className={`
                flex items-center gap-3 px-3 py-2.5 rounded-xl text-sm font-medium
                transition-all duration-200 group relative
                ${
                  isActive
                    ? "bg-[var(--accent-primary-soft)] text-[var(--accent-primary)]"
                    : "text-[var(--text-secondary)] hover:bg-[var(--bg-hover)] hover:text-[var(--text-primary)]"
                }
              `}
            >
              {isActive && (
                <motion.div
                  layoutId="sidebar-active"
                  className="absolute left-0 top-1/2 -translate-y-1/2 w-1 h-6 rounded-r-full bg-[var(--accent-primary)]"
                  transition={{ type: "spring", stiffness: 300, damping: 30 }}
                />
              )}
              <span className="shrink-0">{item.icon}</span>
              <AnimatePresence>
                {!collapsed && (
                  <motion.span
                    initial={{ opacity: 0, width: 0 }}
                    animate={{ opacity: 1, width: "auto" }}
                    exit={{ opacity: 0, width: 0 }}
                    className="overflow-hidden whitespace-nowrap"
                  >
                    {item.label}
                  </motion.span>
                )}
              </AnimatePresence>
            </Link>
          );
        })}
      </nav>

      {/* Collapse toggle (desktop) */}
      <div className="hidden lg:block border-t border-[var(--border-primary)] p-3">
        <button
          onClick={onToggle}
          className="w-full flex items-center justify-center gap-2 px-3 py-2.5 rounded-xl text-sm text-[var(--text-tertiary)] hover:bg-[var(--bg-hover)] hover:text-[var(--text-primary)] transition-colors cursor-pointer"
        >
          <motion.div
            animate={{ rotate: collapsed ? 180 : 0 }}
            transition={{ duration: 0.3 }}
          >
            <ChevronLeft size={18} />
          </motion.div>
          <AnimatePresence>
            {!collapsed && (
              <motion.span
                initial={{ opacity: 0 }}
                animate={{ opacity: 1 }}
                exit={{ opacity: 0 }}
              >
                Collapse
              </motion.span>
            )}
          </AnimatePresence>
        </button>
      </div>
    </div>
  );

  return (
    <>
      {/* Desktop Sidebar */}
      <motion.aside
        animate={{ width: collapsed ? 72 : 260 }}
        transition={{ duration: 0.3, ease: "easeInOut" }}
        className="hidden lg:flex flex-col h-full border-r border-[var(--border-primary)] bg-[var(--bg-card)] backdrop-blur-sm shrink-0 overflow-hidden"
      >
        {sidebarContent}
      </motion.aside>

      {/* Mobile Sidebar */}
      <AnimatePresence>
        {mobileOpen && (
          <motion.aside
            initial={{ x: -280 }}
            animate={{ x: 0 }}
            exit={{ x: -280 }}
            transition={{ duration: 0.3, ease: "easeInOut" }}
            className="fixed inset-y-0 left-0 z-50 w-[260px] flex flex-col border-r border-[var(--border-primary)] bg-[var(--bg-card)] backdrop-blur-xl lg:hidden"
          >
            {/* Mobile close button */}
            <button
              onClick={onMobileClose}
              className="absolute top-4 right-4 p-1.5 rounded-lg text-[var(--text-tertiary)] hover:bg-[var(--bg-hover)] cursor-pointer"
            >
              <X size={18} />
            </button>
            {sidebarContent}
          </motion.aside>
        )}
      </AnimatePresence>
    </>
  );
}
