"use client";

import { useEffect, useState } from "react";
import { useAuth } from "@/lib/auth-context";
import { pointsApi } from "@/lib/api";
import type { AccountLookupResponse, PointHistoryResDTO } from "@/lib/types";
import { Card, CardHeader } from "@/components/ui/Card";
import { Badge } from "@/components/ui/Badge";
import { SkeletonCard, SkeletonTable } from "@/components/ui/Skeleton";
import { AnimatedPage } from "@/components/ui/AnimatedPage";
import { toast } from "@/components/ui/Toast";
import { Star, Zap, Gift, History, Shield, User, Phone } from "lucide-react";

export default function MyPointsPage() {
  const { user, isCustomer } = useAuth();
  const [account, setAccount] = useState<AccountLookupResponse | null>(null);
  const [history, setHistory] = useState<PointHistoryResDTO[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    async function load() {
      if (!user) return;
      setIsLoading(true);
      try {
        const accRes = await pointsApi.lookupAccount(user.mobileNum);
        if (accRes.isSuccess && accRes.data) {
          setAccount(accRes.data);
          // Load history using the account ID
          const histRes = await pointsApi.getPointHistory(accRes.data.accountId);
          if (histRes.isSuccess && histRes.data) setHistory(histRes.data);
        }
      } catch {
        /* may not have a loyalty account yet */
      } finally {
        setIsLoading(false);
      }
    }
    void load();
  }, [user]);

  if (!isCustomer) {
    return (
      <AnimatedPage>
        <div className="flex flex-col items-center justify-center h-[60vh] gap-4">
          <Shield size={48} className="text-[var(--text-tertiary)]" />
          <h2 className="text-xl font-semibold text-[var(--text-primary)]">Customer Only</h2>
          <p className="text-[var(--text-secondary)]">This page is available for customer accounts only.</p>
        </div>
      </AnimatedPage>
    );
  }

  if (isLoading) {
    return (
      <AnimatedPage>
        <div className="space-y-6">
          <div>
            <h2 className="text-2xl font-bold text-[var(--text-primary)]">My Points</h2>
          </div>
          <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
            {Array.from({ length: 3 }).map((_, i) => (<SkeletonCard key={i} />))}
          </div>
          <SkeletonTable rows={5} />
        </div>
      </AnimatedPage>
    );
  }

  return (
    <AnimatedPage>
      <div className="space-y-6">
        {/* Header with Username */}
        <div>
          <h2 className="text-2xl font-bold text-[var(--text-primary)]">My Points</h2>
          <p className="text-sm text-[var(--text-secondary)] mt-1">
            Welcome back! Here&apos;s your loyalty overview.
          </p>
        </div>

        {/* User Identity Card */}
        <Card padding="lg" className="relative overflow-hidden">
          <div className="absolute top-0 right-0 w-40 h-40 rounded-full bg-gradient-to-br from-[var(--accent-primary)] to-purple-500 opacity-[0.07] -translate-y-1/2 translate-x-1/2" />
          <div className="flex items-center gap-4">
            <div className="w-16 h-16 rounded-2xl gradient-primary flex items-center justify-center text-white shadow-lg shrink-0">
              <span className="text-2xl font-bold">{(user?.username || "U").charAt(0)}</span>
            </div>
            <div>
              <h3 className="text-xl font-bold text-[var(--text-primary)]">{user?.username}</h3>
              <div className="flex items-center gap-3 mt-1.5">
                <span className="flex items-center gap-1.5 text-sm text-[var(--text-secondary)]">
                  <Phone size={14} className="text-[var(--text-tertiary)]" />
                  {user?.mobileNum}
                </span>
                {account && <Badge variant="primary">{account.tier || "None"} Tier</Badge>}
              </div>
            </div>
          </div>
        </Card>

        {account ? (
          <>
            {/* Points KPI Cards */}
            <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
              <Card hover className="relative overflow-hidden">
                <div className="absolute top-0 right-0 w-24 h-24 rounded-full bg-gradient-to-br from-amber-500 to-orange-500 opacity-[0.08] -translate-y-1/2 translate-x-1/2" />
                <div className="flex items-start justify-between">
                  <div>
                    <p className="text-sm text-[var(--text-secondary)]">Points Balance</p>
                    <p className="text-3xl font-bold text-[var(--accent-warning)] mt-1">{account.currentBalance}</p>
                    <p className="text-xs text-[var(--text-tertiary)] mt-1">Available to redeem</p>
                  </div>
                  <div className="w-11 h-11 rounded-xl bg-gradient-to-br from-amber-500 to-orange-500 flex items-center justify-center shadow-md">
                    <Star size={22} className="text-white" />
                  </div>
                </div>
              </Card>

              <Card hover className="relative overflow-hidden">
                <div className="absolute top-0 right-0 w-24 h-24 rounded-full bg-gradient-to-br from-violet-500 to-purple-500 opacity-[0.08] -translate-y-1/2 translate-x-1/2" />
                <div className="flex items-start justify-between">
                  <div>
                    <p className="text-sm text-[var(--text-secondary)]">Lifetime Points</p>
                    <p className="text-3xl font-bold text-[var(--text-primary)] mt-1">{account.lifetimePoints}</p>
                    <p className="text-xs text-[var(--text-tertiary)] mt-1">Total earned all time</p>
                  </div>
                  <div className="w-11 h-11 rounded-xl bg-gradient-to-br from-violet-500 to-purple-500 flex items-center justify-center shadow-md">
                    <Zap size={22} className="text-white" />
                  </div>
                </div>
              </Card>

              <Card hover className="relative overflow-hidden">
                <div className="absolute top-0 right-0 w-24 h-24 rounded-full bg-gradient-to-br from-emerald-500 to-teal-500 opacity-[0.08] -translate-y-1/2 translate-x-1/2" />
                <div className="flex items-start justify-between">
                  <div>
                    <p className="text-sm text-[var(--text-secondary)]">My Tier</p>
                    <p className="text-3xl font-bold text-[var(--text-primary)] mt-1">{account.tier || "None"}</p>
                    <p className="text-xs text-[var(--text-tertiary)] mt-1">Membership level</p>
                  </div>
                  <div className="w-11 h-11 rounded-xl bg-gradient-to-br from-emerald-500 to-teal-500 flex items-center justify-center shadow-md">
                    <Gift size={22} className="text-white" />
                  </div>
                </div>
              </Card>
            </div>

            {/* Transaction History */}
            <Card padding="lg">
              <CardHeader
                title="Transaction History"
                subtitle={`${history.length} transactions recorded`}
                action={<History size={18} className="text-[var(--text-tertiary)]" />}
              />
              
              {history.length === 0 ? (
                <div className="py-16 text-center">
                  <History size={48} className="mx-auto mb-3 text-[var(--text-tertiary)] opacity-50" />
                  <p className="text-[var(--text-secondary)]">No transaction history yet</p>
                  <p className="text-xs text-[var(--text-tertiary)] mt-1">Your points activity will show up here</p>
                </div>
              ) : (
                <div className="space-y-2">
                  {history.map((item, idx) => (
                    <div key={idx} className="flex items-center justify-between p-4 rounded-xl border border-[var(--border-primary)] bg-[var(--bg-secondary)] hover:bg-[var(--bg-hover)] transition-colors">
                      <div className="min-w-0 flex-1">
                        <div className="flex items-center gap-2">
                          <div className={`w-8 h-8 rounded-lg flex items-center justify-center shrink-0 ${item.pointDelta >= 0 ? "bg-[var(--accent-success-soft)]" : "bg-[var(--accent-danger-soft)]"}`}>
                            {item.pointDelta >= 0 ? (
                              <Zap size={16} className="text-[var(--accent-success)]" />
                            ) : (
                              <Gift size={16} className="text-[var(--accent-danger)]" />
                            )}
                          </div>
                          <div className="min-w-0">
                            <p className="text-sm font-medium text-[var(--text-primary)] truncate">{item.description}</p>
                            <p className="text-xs text-[var(--text-tertiary)] mt-0.5">
                              {new Date(item.createdAt).toLocaleDateString("en-US", { year: "numeric", month: "short", day: "numeric", hour: "2-digit", minute: "2-digit" })}
                              <span className="mx-1.5">•</span>
                              {item.eventKey}
                            </p>
                          </div>
                        </div>
                      </div>
                      <div className="text-right shrink-0 ml-4">
                        <p className={`text-sm font-bold ${item.pointDelta >= 0 ? "text-[var(--accent-success)]" : "text-[var(--accent-danger)]"}`}>
                          {item.pointDelta >= 0 ? "+" : ""}{item.pointDelta} pts
                        </p>
                        <p className="text-[10px] text-[var(--text-tertiary)] font-mono mt-0.5">{item.referenceId}</p>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </Card>
          </>
        ) : (
          <Card padding="lg" className="text-center py-16">
            <Gift size={48} className="mx-auto mb-4 text-[var(--text-tertiary)] opacity-50" />
            <h3 className="text-lg font-semibold text-[var(--text-primary)]">No Loyalty Account</h3>
            <p className="text-sm text-[var(--text-secondary)] mt-2 max-w-md mx-auto">
              You don&apos;t have a loyalty account yet. Please contact a staff member to set one up for you.
            </p>
          </Card>
        )}
      </div>
    </AnimatedPage>
  );
}
