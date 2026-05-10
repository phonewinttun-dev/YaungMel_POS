"use client";

import { AnimatedPage } from "@/components/ui/AnimatedPage";
import { Badge } from "@/components/ui/Badge";
import { Card, CardHeader } from "@/components/ui/Card";
import { SkeletonCard } from "@/components/ui/Skeleton";
import { toast } from "@/components/ui/Toast";
import { dashboardApi } from "@/lib/api";
import { useAuth } from "@/lib/auth-context";
import type {
  SalesOverviewDTO,
  SalesPerPeriodDTO,
  TopProductDTO,
} from "@/lib/types";
import {
  Calendar,
  DollarSign,
  Gift,
  Package,
  Receipt,
  ShoppingCart,
  Tags,
  TrendingUp,
  Warehouse,
} from "lucide-react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useCallback, useEffect, useState } from "react";
import {
  Area,
  AreaChart,
  Bar,
  BarChart,
  CartesianGrid,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";

// ═══════════════════════════════════════════════════════
// Admin Dashboard
// ═══════════════════════════════════════════════════════
function AdminDashboard() {
  const [overview, setOverview] = useState<SalesOverviewDTO | null>(null);
  const [salesPeriod, setSalesPeriod] = useState<SalesPerPeriodDTO | null>(null);
  const [topProducts, setTopProducts] = useState<TopProductDTO[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [period, setPeriod] = useState("day");

  const loadDashboard = useCallback(async () => {
    setIsLoading(true);
    try {
      const endDate = new Date().toISOString();
      const startDate = new Date(
        Date.now() - 30 * 24 * 60 * 60 * 1000
      ).toISOString();

      const [overviewRes, topRes] = await Promise.all([
        dashboardApi.getOverview(startDate, endDate),
        dashboardApi.getTopProducts(5),
      ]);

      if (overviewRes.isSuccess && overviewRes.data) setOverview(overviewRes.data);
      if (topRes.isSuccess && topRes.data) setTopProducts(topRes.data);
    } catch {
      toast("error", "Failed to load dashboard data");
    } finally {
      setIsLoading(false);
    }
  }, []);

  const loadSalesPeriod = useCallback(async (p: string) => {
    try {
      const res = await dashboardApi.getSalesPerPeriod(p);
      if (res.isSuccess && res.data) setSalesPeriod(res.data);
    } catch {
      /* silent */
    }
  }, []);

  useEffect(() => {
    void loadDashboard();
  }, [loadDashboard]);

  useEffect(() => {
    void loadSalesPeriod(period);
  }, [period, loadSalesPeriod]);

  const kpiCards = [
    {
      label: "Total Revenue",
      value: overview
        ? `${overview.totalRevenue.toLocaleString()} MMK`
        : "0 MMK",
      icon: DollarSign,
      color: "from-blue-500 to-cyan-500",
    },
    {
      label: "Total Sales",
      value: overview?.totalSales?.toLocaleString() || "0",
      icon: ShoppingCart,
      color: "from-violet-500 to-purple-500",
    },
    {
      label: "Avg. per Sale",
      value:
        overview && overview.totalSales > 0
          ? `${(overview.totalRevenue / overview.totalSales).toLocaleString()} MMK`
          : "0 MMK",
      icon: TrendingUp,
      color: "from-emerald-500 to-teal-500",
    },
    {
      label: "Top Products",
      value: topProducts.length.toString(),
      icon: Package,
      color: "from-amber-500 to-orange-500",
    },
  ];

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-2xl font-bold text-[var(--text-primary)]">
          Dashboard Overview
        </h2>
        <p className="text-sm text-[var(--text-secondary)] mt-1">
          Last 30 days performance summary
        </p>
      </div>

      {/* KPI Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        {isLoading
          ? Array.from({ length: 4 }).map((_, i) => <SkeletonCard key={i} />)
          : kpiCards.map((kpi) => {
              const Icon = kpi.icon;
              return (
                <Card key={kpi.label} hover className="relative overflow-hidden">
                  <div className="flex items-start justify-between">
                    <div>
                      <p className="text-sm text-[var(--text-secondary)]">
                        {kpi.label}
                      </p>
                      <p className="text-2xl font-bold text-[var(--text-primary)] mt-1">
                        {kpi.value}
                      </p>
                    </div>
                    <div
                      className={`w-11 h-11 rounded-xl bg-gradient-to-br ${kpi.color} flex items-center justify-center shadow-md`}
                    >
                      <Icon size={22} className="text-white" />
                    </div>
                  </div>
                </Card>
              );
            })}
      </div>

      {/* Charts Row */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Revenue Chart */}
        <Card className="lg:col-span-2" padding="lg">
          <CardHeader
            title="Revenue Trend"
            subtitle={`Grouped by ${period}`}
            action={
              <div className="flex gap-1 bg-[var(--bg-tertiary)] rounded-xl p-1">
                {["day", "week", "month"].map((p) => (
                  <button
                    key={p}
                    onClick={() => setPeriod(p)}
                    className={`px-3 py-1.5 text-xs font-medium rounded-lg transition-all duration-200 cursor-pointer
                      ${
                        period === p
                          ? "bg-[var(--accent-primary)] text-white shadow-sm"
                          : "text-[var(--text-secondary)] hover:text-[var(--text-primary)]"
                      }
                    `}
                  >
                    {p.charAt(0).toUpperCase() + p.slice(1)}
                  </button>
                ))}
              </div>
            }
          />
          <div className="h-72">
            {salesPeriod && salesPeriod.data.length > 0 ? (
              <ResponsiveContainer width="100%" height="100%">
                <AreaChart data={salesPeriod.data}>
                  <defs>
                    <linearGradient id="colorRevenue" x1="0" y1="0" x2="0" y2="1">
                      <stop offset="5%" stopColor="#6366f1" stopOpacity={0.3} />
                      <stop offset="95%" stopColor="#6366f1" stopOpacity={0} />
                    </linearGradient>
                  </defs>
                  <CartesianGrid strokeDasharray="3 3" stroke="var(--border-primary)" />
                  <XAxis
                    dataKey="label"
                    tick={{ fill: "var(--text-tertiary)", fontSize: 12 }}
                    tickLine={false}
                    axisLine={{ stroke: "var(--border-primary)" }}
                  />
                  <YAxis
                    tick={{ fill: "var(--text-tertiary)", fontSize: 12 }}
                    tickLine={false}
                    axisLine={false}
                  />
                  <Tooltip
                    contentStyle={{
                      backgroundColor: "var(--bg-card)",
                      border: "1px solid var(--border-primary)",
                      borderRadius: "12px",
                      boxShadow: "var(--shadow-lg)",
                    }}
                    labelStyle={{ color: "var(--text-primary)", fontWeight: 600 }}
                    itemStyle={{ color: "var(--text-secondary)" }}
                  />
                  <Area
                    type="monotone"
                    dataKey="totalRevenue"
                    stroke="#6366f1"
                    strokeWidth={2}
                    fill="url(#colorRevenue)"
                    name="Revenue"
                  />
                </AreaChart>
              </ResponsiveContainer>
            ) : (
              <div className="h-full flex items-center justify-center text-[var(--text-tertiary)]">
                <div className="text-center">
                  <Calendar size={40} className="mx-auto mb-2 opacity-50" />
                  <p className="text-sm">No sales data available</p>
                </div>
              </div>
            )}
          </div>
        </Card>

        {/* Top Products Chart */}
        <Card padding="lg">
          <CardHeader title="Top Products" subtitle="By quantity sold" />
          <div className="h-72">
            {topProducts.length > 0 ? (
              <ResponsiveContainer width="100%" height="100%">
                <BarChart data={topProducts} layout="vertical">
                  <CartesianGrid strokeDasharray="3 3" stroke="var(--border-primary)" />
                  <XAxis
                    type="number"
                    tick={{ fill: "var(--text-tertiary)", fontSize: 12 }}
                    tickLine={false}
                    axisLine={false}
                  />
                  <YAxis
                    type="category"
                    dataKey="productName"
                    tick={{ fill: "var(--text-secondary)", fontSize: 12 }}
                    tickLine={false}
                    axisLine={false}
                    width={80}
                  />
                  <Tooltip
                    contentStyle={{
                      backgroundColor: "var(--bg-card)",
                      border: "1px solid var(--border-primary)",
                      borderRadius: "12px",
                    }}
                    labelStyle={{ color: "var(--text-primary)", fontWeight: 600 }}
                  />
                  <Bar
                    dataKey="totalQuantitySold"
                    fill="#8b5cf6"
                    radius={[0, 6, 6, 0]}
                    name="Qty Sold"
                  />
                </BarChart>
              </ResponsiveContainer>
            ) : (
              <div className="h-full flex items-center justify-center text-[var(--text-tertiary)]">
                <div className="text-center">
                  <Package size={40} className="mx-auto mb-2 opacity-50" />
                  <p className="text-sm">No product data yet</p>
                </div>
              </div>
            )}
          </div>
        </Card>
      </div>

      {/* Top Products Table */}
      <Card padding="lg">
        <CardHeader title="Top Selling Products" subtitle="Detailed breakdown" />
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead>
              <tr className="border-b border-[var(--border-primary)]">
                {["Product", "Qty Sold", "Revenue", "Status"].map((h) => (
                  <th
                    key={h}
                    className="text-left py-3 px-4 text-xs font-semibold text-[var(--text-tertiary)] uppercase tracking-wider last:text-right"
                  >
                    {h}
                  </th>
                ))}
              </tr>
            </thead>
            <tbody>
              {topProducts.length > 0 ? (
                topProducts.map((product, index) => (
                  <tr
                    key={product.productId}
                    className="border-b border-[var(--border-primary)] last:border-0 hover:bg-[var(--bg-hover)] transition-colors"
                  >
                    <td className="py-3 px-4">
                      <div className="flex items-center gap-3">
                        <span className="w-7 h-7 rounded-lg bg-gradient-to-br from-blue-500 to-violet-500 flex items-center justify-center text-white text-xs font-bold">
                          {index + 1}
                        </span>
                        <span className="text-sm font-medium text-[var(--text-primary)]">
                          {product.productName}
                        </span>
                      </div>
                    </td>
                    <td className="py-3 px-4 text-right">
                      <span className="text-sm text-[var(--text-primary)] font-mono">
                        {product.totalQuantitySold}
                      </span>
                    </td>
                    <td className="py-3 px-4 text-right">
                      <span className="text-sm text-[var(--text-primary)] font-mono">
                        {product.totalRevenue.toLocaleString()} MMK
                      </span>
                    </td>
                    <td className="py-3 px-4 text-right">
                      <Badge variant={index === 0 ? "success" : index < 3 ? "primary" : "default"}>
                        {index === 0 ? "Top Seller" : `#${index + 1}`}
                      </Badge>
                    </td>
                  </tr>
                ))
              ) : (
                <tr>
                  <td colSpan={4} className="py-12 text-center text-[var(--text-tertiary)]">
                    No sales data available yet
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </Card>
    </div>
  );
}

// ═══════════════════════════════════════════════════════
// Staff Dashboard
// ═══════════════════════════════════════════════════════
function StaffDashboard() {
  const quickActions = [
    {
      label: "New Sale",
      description: "Process a transaction at the point of sale",
      href: "/pos",
      icon: ShoppingCart,
      color: "from-blue-500 to-cyan-500",
    },
    {
      label: "Products",
      description: "View and manage the product catalog",
      href: "/products",
      icon: Package,
      color: "from-violet-500 to-purple-500",
    },
    {
      label: "Inventory",
      description: "Check stock levels and adjust quantities",
      href: "/inventory",
      icon: Warehouse,
      color: "from-emerald-500 to-teal-500",
    },
    {
      label: "Sales History",
      description: "View past transactions and vouchers",
      href: "/sales",
      icon: Receipt,
      color: "from-amber-500 to-orange-500",
    },
    {
      label: "Categories",
      description: "Manage product categories",
      href: "/categories",
      icon: Tags,
      color: "from-pink-500 to-rose-500",
    },
    {
      label: "Loyalty",
      description: "Manage loyalty accounts and rewards",
      href: "/loyalty",
      icon: Gift,
      color: "from-indigo-500 to-blue-500",
    },
  ];

  const [shortcuts, setShortcuts] = useState([
    {
      label: "Reports",
      description: "View detailed sales reports",
      href: "/reports",
      icon: TrendingUp,
      color: "from-green-500 to-lime-500",
    },
  ]);

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-2xl font-bold text-[var(--text-primary)]">
          Staff Dashboard
        </h2>
        <p className="text-sm text-[var(--text-secondary)] mt-1">
          Quick actions to get things done
        </p>
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
        {quickActions.map((action) => {
          const Icon = action.icon;
          return (
            <Link key={action.href} href={action.href}>
              <Card hover className="h-full cursor-pointer group">
                <div className="flex items-start gap-4">
                  <div
                    className={`w-12 h-12 rounded-xl bg-gradient-to-br ${action.color} flex items-center justify-center shadow-md shrink-0 group-hover:scale-110 transition-transform duration-200`}
                  >
                    <Icon size={24} className="text-white" />
                  </div>
                  <div>
                    <h3 className="text-base font-semibold text-[var(--text-primary)] group-hover:text-[var(--accent-primary)] transition-colors">
                      {action.label}
                    </h3>
                    <p className="text-xs text-[var(--text-tertiary)] mt-1">
                      {action.description}
                    </p>
                  </div>
                </div>
              </Card>
            </Link>
          );
        })}
      </div>

      {/* Shortcut Section */}
      <div>
        <h3 className="text-lg font-semibold text-[var(--text-primary)] mt-6">
          Shortcuts
        </h3>
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4 mt-4">
          {shortcuts.map((shortcut) => {
            const Icon = shortcut.icon;
            return (
              <Link key={shortcut.href} href={shortcut.href}>
                <Card hover className="h-full cursor-pointer group">
                  <div className="flex items-start gap-4">
                    <div
                      className={`w-12 h-12 rounded-xl bg-gradient-to-br ${shortcut.color} flex items-center justify-center shadow-md shrink-0 group-hover:scale-110 transition-transform duration-200`}
                    >
                      <Icon size={24} className="text-white" />
                    </div>
                    <div>
                      <h3 className="text-base font-semibold text-[var(--text-primary)] group-hover:text-[var(--accent-primary)] transition-colors">
                        {shortcut.label}
                      </h3>
                      <p className="text-xs text-[var(--text-tertiary)] mt-1">
                        {shortcut.description}
                      </p>
                    </div>
                  </div>
                </Card>
              </Link>
            );
          })}
        </div>
      </div>
    </div>
  );
}

// ═══════════════════════════════════════════════════════
// Main Dashboard Page (Role Router)
// ═══════════════════════════════════════════════════════
export default function DashboardPage() {
  const { isAdmin, isStaff, isCustomer } = useAuth();
  const router = useRouter();

  // Redirect customers to My Points page
  useEffect(() => {
    if (isCustomer) {
      router.replace("/my-points");
    }
  }, [isCustomer, router]);

  return (
    <AnimatedPage>
      {isAdmin && <AdminDashboard />}
      {isStaff && <StaffDashboard />}
      {isCustomer && (
        <div className="flex items-center justify-center h-[60vh]">
          <div className="w-10 h-10 border-3 border-[var(--accent-primary)] border-t-transparent rounded-full animate-spin" />
        </div>
      )}
    </AnimatedPage>
  );
}
