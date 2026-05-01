"use client";

import { useEffect, useState } from "react";
import { useAuth } from "@/lib/auth-context";
import { pointsApi } from "@/lib/api";
import type { AvailableRewardResDTO, AccountLookupResponse, ClaimRewardResDTO } from "@/lib/types";
import { Card, CardHeader } from "@/components/ui/Card";
import { Input } from "@/components/ui/Input";
import { Button } from "@/components/ui/Button";
import { Badge } from "@/components/ui/Badge";
import { Modal } from "@/components/ui/Modal";
import { SkeletonTable } from "@/components/ui/Skeleton";
import { AnimatedPage } from "@/components/ui/AnimatedPage";
import { toast } from "@/components/ui/Toast";
import { Gift, Search as SearchIcon, UserCheck, Coins, Package, CheckCircle, Shield, AlertTriangle, ArrowRight } from "lucide-react";

export default function LoyaltyPage() {
  const { isAdmin, isStaff } = useAuth();
  const [rewards, setRewards] = useState<AvailableRewardResDTO[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  // Step state: "search" -> "rewards" -> "confirm" -> "done"
  const [step, setStep] = useState<"search" | "rewards" | "confirm" | "done">("search");
  const [externalId, setExternalId] = useState("");
  const [lookupLoading, setLookupLoading] = useState(false);
  const [customerAccount, setCustomerAccount] = useState<AccountLookupResponse | null>(null);
  const [selectedReward, setSelectedReward] = useState<AvailableRewardResDTO | null>(null);
  const [claimLoading, setClaimLoading] = useState(false);
  const [claimResult, setClaimResult] = useState<ClaimRewardResDTO | null>(null);
  const [notes, setNotes] = useState("");

  useEffect(() => {
    const timer = window.setTimeout(async () => {
      setIsLoading(true);
      try {
        const rewardsRes = await pointsApi.getAvailableRewards();
        if (rewardsRes.isSuccess && rewardsRes.data) setRewards(rewardsRes.data);
      } catch {
        toast("error", "Failed to load rewards");
      } finally {
        setIsLoading(false);
      }
    }, 0);
    return () => window.clearTimeout(timer);
  }, []);

  const handleLookup = async () => {
    if (!externalId.trim()) {
      toast("error", "Please enter an External User ID");
      return;
    }
    setLookupLoading(true);
    try {
      const res = await pointsApi.lookupAccount(externalId.trim());
      if (res.isSuccess && res.data) {
        setCustomerAccount(res.data);
        setStep("rewards");
        toast("success", `Customer found: ${res.data.externalUserId}`);
      } else {
        toast("error", res.message || "Customer not found. Please check the ID.");
      }
    } catch {
      toast("error", "Customer not found. Please check the External User ID.");
    } finally {
      setLookupLoading(false);
    }
  };

  const handleSelectReward = (reward: AvailableRewardResDTO) => {
    if (!customerAccount) return;
    if (customerAccount.currentBalance < reward.pointCost) {
      toast("error", `Insufficient points. Customer has ${customerAccount.currentBalance} pts, needs ${reward.pointCost} pts.`);
      return;
    }
    if (reward.stockQuantity <= 0) {
      toast("error", "This reward is out of stock.");
      return;
    }
    setSelectedReward(reward);
    setStep("confirm");
  };

  const handleClaimReward = async () => {
    if (!selectedReward || !customerAccount) return;
    setClaimLoading(true);
    try {
      const res = await pointsApi.claimReward({
        externalUserId: customerAccount.externalUserId,
        rewardId: selectedReward.id,
        notes: notes || `Claimed by staff for ${customerAccount.externalUserId}`,
      });
      if (res.isSuccess && res.data) {
        setClaimResult(res.data);
        setStep("done");
        toast("success", "Reward claimed successfully!");
      } else {
        toast("error", res.message || "Failed to claim reward");
      }
    } catch {
      toast("error", "Failed to claim reward");
    } finally {
      setClaimLoading(false);
    }
  };

  const handleReset = () => {
    setStep("search");
    setExternalId("");
    setCustomerAccount(null);
    setSelectedReward(null);
    setClaimResult(null);
    setNotes("");
  };

  if (!isAdmin && !isStaff) {
    return (
      <AnimatedPage>
        <div className="flex flex-col items-center justify-center h-[60vh] gap-4">
          <Shield size={48} className="text-[var(--text-tertiary)]" />
          <h2 className="text-xl font-semibold text-[var(--text-primary)]">Access Restricted</h2>
          <p className="text-[var(--text-secondary)]">Loyalty management is available for Admin and Staff only.</p>
        </div>
      </AnimatedPage>
    );
  }

  return (
    <AnimatedPage>
      <div className="space-y-6">
        <div>
          <h2 className="text-2xl font-bold text-[var(--text-primary)]">Reward Claiming</h2>
          <p className="text-sm text-[var(--text-secondary)] mt-1">Claim rewards for customers using their External User ID.</p>
        </div>

        {/* Progress Steps */}
        <div className="flex items-center gap-2">
          {[
            { key: "search", label: "1. Find Customer", icon: SearchIcon },
            { key: "rewards", label: "2. Choose Reward", icon: Gift },
            { key: "confirm", label: "3. Confirm", icon: CheckCircle },
          ].map((s, i) => {
            const Icon = s.icon;
            const isActive = step === s.key || (step === "done" && s.key === "confirm");
            const isPast =
              (step === "rewards" && i === 0) ||
              (step === "confirm" && i <= 1) ||
              step === "done";
            return (
              <div key={s.key} className="flex items-center gap-2">
                {i > 0 && <ArrowRight size={14} className={`${isPast ? "text-[var(--accent-primary)]" : "text-[var(--text-tertiary)]"}`} />}
                <div className={`flex items-center gap-2 px-3 py-2 rounded-xl text-sm font-medium transition-all duration-200 ${
                  isActive ? "bg-[var(--accent-primary-soft)] text-[var(--accent-primary)]" :
                  isPast ? "text-[var(--accent-success)]" :
                  "text-[var(--text-tertiary)]"
                }`}>
                  <Icon size={16} />
                  <span className="hidden sm:inline">{s.label}</span>
                </div>
              </div>
            );
          })}
        </div>

        {/* Step 1: Search by External ID */}
        {step === "search" && (
          <Card padding="lg">
            <CardHeader title="Find Customer" subtitle="Enter the customer's External User ID to look up their account." action={<SearchIcon size={18} className="text-[var(--text-tertiary)]" />} />
            <div className="flex flex-col sm:flex-row gap-3">
              <div className="flex-1">
                <Input
                  placeholder="Enter External User ID..."
                  value={externalId}
                  onChange={(e) => setExternalId(e.target.value)}
                  onKeyDown={(e) => e.key === "Enter" && handleLookup()}
                  icon={<SearchIcon size={16} />}
                  id="loyalty-external-id"
                />
              </div>
              <Button onClick={handleLookup} isLoading={lookupLoading} icon={<UserCheck size={16} />}>Look Up</Button>
            </div>
          </Card>
        )}

        {/* Step 2: Choose Reward */}
        {step === "rewards" && customerAccount && (
          <>
            {/* Customer Info */}
            <Card padding="lg" className="relative overflow-hidden">
              <div className="absolute top-0 right-0 w-32 h-32 rounded-full bg-gradient-to-br from-[var(--accent-primary)] to-purple-500 opacity-[0.06] -translate-y-1/2 translate-x-1/2" />
              <div className="flex items-center justify-between flex-wrap gap-4">
                <div className="flex items-center gap-4">
                  <div className="w-12 h-12 rounded-xl gradient-primary flex items-center justify-center text-white shadow-md">
                    <span className="text-lg font-bold">{customerAccount.externalUserId.charAt(0).toUpperCase()}</span>
                  </div>
                  <div>
                    <h3 className="text-lg font-bold text-[var(--text-primary)]">{customerAccount.externalUserId}</h3>
                    <div className="flex items-center gap-2 mt-1">
                      <Badge variant="primary">{customerAccount.tier || "None"}</Badge>
                      <span className="text-sm text-[var(--text-secondary)]">Balance: <strong className="text-[var(--accent-warning)]">{customerAccount.currentBalance} pts</strong></span>
                    </div>
                  </div>
                </div>
                <Button variant="secondary" size="sm" onClick={handleReset}>Change Customer</Button>
              </div>
            </Card>

            {/* Available Rewards */}
            <Card padding="lg">
              <CardHeader title="Available Rewards" subtitle={`${rewards.length} rewards available. Select one to claim.`} action={<Gift size={18} className="text-[var(--text-tertiary)]" />} />

              {isLoading ? (
                <SkeletonTable rows={4} />
              ) : rewards.length === 0 ? (
                <div className="py-12 text-center text-[var(--text-secondary)]">No active rewards available.</div>
              ) : (
                <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
                  {rewards.map((reward) => {
                    const canAfford = customerAccount.currentBalance >= reward.pointCost;
                    const inStock = reward.stockQuantity > 0;
                    return (
                      <button
                        key={reward.id}
                        onClick={() => handleSelectReward(reward)}
                        disabled={!canAfford || !inStock}
                        className={`text-left p-5 rounded-2xl border transition-all duration-200 cursor-pointer ${
                          canAfford && inStock
                            ? "bg-[var(--bg-card)] border-[var(--border-primary)] hover:border-[var(--accent-primary)] hover:shadow-md hover:bg-[var(--accent-primary-soft)]"
                            : "bg-[var(--bg-secondary)] border-[var(--border-primary)] opacity-60 cursor-not-allowed"
                        }`}
                      >
                        <div className="flex items-start justify-between gap-3">
                          <div>
                            <h3 className="text-base font-semibold text-[var(--text-primary)]">{reward.name}</h3>
                            <p className="text-sm text-[var(--text-secondary)] mt-1">{reward.description}</p>
                          </div>
                          <Badge variant={reward.isActive ? "success" : "danger"}>{reward.isActive ? "Active" : "Inactive"}</Badge>
                        </div>
                        <div className="flex items-center gap-3 mt-4">
                          <div className="flex items-center gap-1.5">
                            <Coins size={14} className="text-[var(--accent-warning)]" />
                            <span className="text-sm font-semibold text-[var(--text-primary)]">{reward.pointCost} pts</span>
                          </div>
                          <div className="flex items-center gap-1.5">
                            <Package size={14} className="text-[var(--accent-info)]" />
                            <span className="text-sm text-[var(--text-secondary)]">{reward.stockQuantity} left</span>
                          </div>
                        </div>
                        {!canAfford && (
                          <p className="text-xs text-[var(--accent-danger)] mt-3 flex items-center gap-1">
                            <AlertTriangle size={12} /> Insufficient points
                          </p>
                        )}
                        {!inStock && (
                          <p className="text-xs text-[var(--accent-danger)] mt-3 flex items-center gap-1">
                            <AlertTriangle size={12} /> Out of stock
                          </p>
                        )}
                      </button>
                    );
                  })}
                </div>
              )}
            </Card>
          </>
        )}

        {/* Step 3: Confirm */}
        {step === "confirm" && selectedReward && customerAccount && (
          <Card padding="lg">
            <CardHeader title="Confirm Reward Claim" subtitle="Review the details before claiming." />
            
            <div className="space-y-4">
              <div className="rounded-2xl border border-[var(--border-primary)] bg-[var(--bg-secondary)] p-5">
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  <div>
                    <p className="text-xs text-[var(--text-tertiary)] uppercase font-semibold">Customer</p>
                    <p className="text-lg font-bold text-[var(--text-primary)] mt-1">{customerAccount.externalUserId}</p>
                  </div>
                  <div>
                    <p className="text-xs text-[var(--text-tertiary)] uppercase font-semibold">Current Balance</p>
                    <p className="text-lg font-bold text-[var(--accent-warning)] mt-1">{customerAccount.currentBalance} pts</p>
                  </div>
                  <div>
                    <p className="text-xs text-[var(--text-tertiary)] uppercase font-semibold">Reward</p>
                    <p className="text-lg font-bold text-[var(--text-primary)] mt-1">{selectedReward.name}</p>
                  </div>
                  <div>
                    <p className="text-xs text-[var(--text-tertiary)] uppercase font-semibold">Cost</p>
                    <p className="text-lg font-bold text-[var(--accent-danger)] mt-1">-{selectedReward.pointCost} pts</p>
                  </div>
                </div>
              </div>

              <div className="rounded-2xl border border-[var(--accent-warning)] border-opacity-30 bg-[var(--accent-warning-soft)] p-4 flex items-start gap-3">
                <AlertTriangle size={18} className="text-[var(--accent-warning)] shrink-0 mt-0.5" />
                <div>
                  <p className="text-sm font-semibold text-[var(--text-primary)]">After claiming:</p>
                  <p className="text-sm text-[var(--text-secondary)]">
                    Balance will be <strong>{customerAccount.currentBalance - selectedReward.pointCost} pts</strong>
                  </p>
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium text-[var(--text-secondary)] mb-1.5">Notes (optional)</label>
                <textarea
                  placeholder="Add any notes about this redemption..."
                  value={notes}
                  onChange={(e) => setNotes(e.target.value)}
                  className="w-full px-4 py-2.5 text-sm rounded-xl bg-[var(--bg-input)] border border-[var(--border-primary)] text-[var(--text-primary)] focus:outline-none focus:ring-2 focus:ring-[var(--accent-primary)] min-h-[60px] resize-none"
                />
              </div>

              <div className="flex gap-3 pt-2">
                <Button variant="secondary" className="flex-1" onClick={() => setStep("rewards")}>Back</Button>
                <Button className="flex-1" onClick={handleClaimReward} isLoading={claimLoading} icon={<CheckCircle size={16} />}>Confirm Claim</Button>
              </div>
            </div>
          </Card>
        )}

        {/* Step 4: Done */}
        {step === "done" && claimResult && (
          <Card padding="lg">
            <div className="text-center space-y-6 py-4">
              <div className="w-20 h-20 rounded-full bg-[var(--accent-success-soft)] flex items-center justify-center mx-auto">
                <CheckCircle size={40} className="text-[var(--accent-success)]" />
              </div>
              <div>
                <h3 className="text-2xl font-bold text-[var(--text-primary)]">Reward Claimed!</h3>
                <p className="text-sm text-[var(--text-secondary)] mt-2">The reward has been successfully claimed for the customer.</p>
              </div>
              <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 max-w-lg mx-auto">
                <div className="p-4 rounded-xl bg-[var(--bg-secondary)] border border-[var(--border-primary)]">
                  <p className="text-xs text-[var(--text-tertiary)] uppercase">Status</p>
                  <p className="text-lg font-bold text-[var(--accent-success)] mt-1">{claimResult.status}</p>
                </div>
                <div className="p-4 rounded-xl bg-[var(--bg-secondary)] border border-[var(--border-primary)]">
                  <p className="text-xs text-[var(--text-tertiary)] uppercase">Remaining</p>
                  <p className="text-lg font-bold text-[var(--text-primary)] mt-1">{claimResult.remainingBalance} pts</p>
                </div>
                <div className="p-4 rounded-xl bg-[var(--bg-secondary)] border border-[var(--border-primary)]">
                  <p className="text-xs text-[var(--text-tertiary)] uppercase">Redemption ID</p>
                  <p className="text-sm font-mono font-bold text-[var(--text-primary)] mt-1 break-all">{claimResult.redemptionId.slice(0, 12)}...</p>
                </div>
              </div>
              <Button onClick={handleReset} className="mx-auto" icon={<SearchIcon size={16} />}>Claim Another Reward</Button>
            </div>
          </Card>
        )}
      </div>
    </AnimatedPage>
  );
}
