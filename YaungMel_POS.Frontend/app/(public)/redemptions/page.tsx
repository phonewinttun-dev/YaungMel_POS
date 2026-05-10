"use client";

import { useEffect, useState, useCallback } from "react";
import { useAuth } from "@/lib/auth-context";
import { pointsApi } from "@/lib/api";
import type { PendingRedemptionResDTO } from "@/lib/types";
import { Card, CardHeader } from "@/components/ui/Card";
import { Button } from "@/components/ui/Button";
import { Badge } from "@/components/ui/Badge";
import { SkeletonTable } from "@/components/ui/Skeleton";
import { AnimatedPage } from "@/components/ui/AnimatedPage";
import { toast } from "@/components/ui/Toast";
import { ClipboardList, RefreshCcw, CheckCircle, Shield, Clock, AlertTriangle } from "lucide-react";

export default function RedemptionsPage() {
  const { isAdmin, isStaff } = useAuth();
  const [redemptions, setRedemptions] = useState<PendingRedemptionResDTO[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isRefreshing, setIsRefreshing] = useState(false);
  const [updatingId, setUpdatingId] = useState<string | null>(null);

  const loadRedemptions = useCallback(async () => {
    try {
      const res = await pointsApi.getPendingRedemptions();
      if (res.isSuccess && res.data) setRedemptions(res.data);
    } catch {
      toast("error", "Failed to load redemptions");
    }
  }, []);

  useEffect(() => {
    setIsLoading(true);
    loadRedemptions().finally(() => setIsLoading(false));
  }, [loadRedemptions]);

  const handleRefresh = async () => {
    setIsRefreshing(true);
    await loadRedemptions();
    setIsRefreshing(false);
    toast("success", "Redemptions refreshed");
  };

  const handleFulfill = async (redemption: PendingRedemptionResDTO) => {
    setUpdatingId(redemption.id);
    try {
      const res = await pointsApi.updateRedemptionStatus(redemption.id, "Fulfilled");
      if (res.isSuccess) {
        toast("success", `Redemption for "${redemption.rewardName}" fulfilled!`);
        await loadRedemptions();
      } else {
        toast("error", res.message || "Failed to fulfill redemption");
      }
    } catch {
      toast("error", "Failed to update redemption status");
    } finally {
      setUpdatingId(null);
    }
  };

  if (!isAdmin && !isStaff) {
    return (
      <AnimatedPage>
        <div className="flex flex-col items-center justify-center h-[60vh] gap-4">
          <Shield size={48} className="text-[var(--text-tertiary)]" />
          <h2 className="text-xl font-semibold text-[var(--text-primary)]">Access Restricted</h2>
          <p className="text-[var(--text-secondary)]">Redemption management is available for Admin and Staff only.</p>
        </div>
      </AnimatedPage>
    );
  }

  return (
    <AnimatedPage>
      <div className="space-y-6">
        <div className="flex items-center justify-between">
          <div>
            <h2 className="text-2xl font-bold text-[var(--text-primary)]">Redemptions</h2>
            <p className="text-sm text-[var(--text-secondary)] mt-1">
              Manage pending reward redemptions. <span className="text-[var(--accent-warning)] font-medium">Award points cannot be cancelled.</span>
            </p>
          </div>
          <Button variant="secondary" size="sm" onClick={handleRefresh} isLoading={isRefreshing} icon={<RefreshCcw size={14} />}>Refresh</Button>
        </div>

        {/* Info Banner */}
        <div className="flex items-start gap-3 p-4 rounded-2xl bg-[var(--accent-warning-soft)] border border-[var(--accent-warning)] border-opacity-30">
          <AlertTriangle size={20} className="text-[var(--accent-warning)] shrink-0 mt-0.5" />
          <div>
            <p className="text-sm font-semibold text-[var(--text-primary)]">Award Points Policy</p>
            <p className="text-xs text-[var(--text-secondary)] mt-0.5">Once points are awarded to a customer, they cannot be cancelled or reversed. Only reward redemptions can be managed here.</p>
          </div>
        </div>

        {/* Redemptions Table */}
        <Card padding="lg">
          <CardHeader
            title="Pending Redemptions"
            subtitle={`${redemptions.length} redemptions awaiting fulfillment`}
            action={<ClipboardList size={18} className="text-[var(--text-tertiary)]" />}
          />

          {isLoading ? (
            <SkeletonTable rows={6} />
          ) : redemptions.length === 0 ? (
            <div className="py-16 text-center">
              <CheckCircle size={48} className="mx-auto mb-3 text-[var(--accent-success)] opacity-50" />
              <p className="text-[var(--text-secondary)]">All caught up!</p>
              <p className="text-xs text-[var(--text-tertiary)] mt-1">No pending redemptions at this time</p>
            </div>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead>
                  <tr className="border-b border-[var(--border-primary)] bg-[var(--bg-secondary)]">
                    {["Customer", "Reward", "Points", "Status", "Redeemed At", ""].map((h) => (
                      <th key={h} className="text-left py-3 px-4 text-xs font-semibold text-[var(--text-tertiary)] uppercase tracking-wider last:text-right">{h}</th>
                    ))}
                  </tr>
                </thead>
                <tbody>
                  {redemptions.map((r) => (
                    <tr key={r.id} className="border-b border-[var(--border-primary)] last:border-0 hover:bg-[var(--bg-hover)] transition-colors">
                      <td className="py-3 px-4">
                        <div>
                          <p className="text-sm font-medium text-[var(--text-primary)]">{r.externalUserId}</p>
                          <p className="text-xs text-[var(--text-tertiary)] font-mono">{r.id.slice(0, 8)}...</p>
                        </div>
                      </td>
                      <td className="py-3 px-4">
                        <p className="text-sm font-medium text-[var(--text-primary)]">{r.rewardName}</p>
                      </td>
                      <td className="py-3 px-4">
                        <Badge variant="primary">{r.pointCost} pts</Badge>
                      </td>
                      <td className="py-3 px-4">
                        <Badge variant={r.status === "Pending" ? "warning" : r.status === "Fulfilled" ? "success" : "danger"}>
                          <span className="flex items-center gap-1">
                            {r.status === "Pending" && <Clock size={12} />}
                            {r.status === "Fulfilled" && <CheckCircle size={12} />}
                            {r.status}
                          </span>
                        </Badge>
                      </td>
                      <td className="py-3 px-4 text-sm text-[var(--text-secondary)]">
                        {new Date(r.redeemedAt).toLocaleDateString("en-US", { month: "short", day: "numeric", year: "numeric", hour: "2-digit", minute: "2-digit" })}
                      </td>
                      <td className="py-3 px-4 text-right">
                        {r.status === "Pending" && (
                          <Button
                            size="sm"
                            onClick={() => handleFulfill(r)}
                            isLoading={updatingId === r.id}
                            icon={<CheckCircle size={14} />}
                          >
                            Fulfill
                          </Button>
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </Card>
      </div>
    </AnimatedPage>
  );
}
