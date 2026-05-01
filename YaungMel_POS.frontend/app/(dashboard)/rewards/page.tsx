"use client";

import { useEffect, useState, useCallback } from "react";
import { useAuth } from "@/lib/auth-context";
import { pointsApi } from "@/lib/api";
import type { AvailableRewardResDTO } from "@/lib/types";
import { Card, CardHeader } from "@/components/ui/Card";
import { Input } from "@/components/ui/Input";
import { Button } from "@/components/ui/Button";
import { Badge } from "@/components/ui/Badge";
import { Modal } from "@/components/ui/Modal";
import { SkeletonTable } from "@/components/ui/Skeleton";
import { AnimatedPage } from "@/components/ui/AnimatedPage";
import { toast } from "@/components/ui/Toast";
import { Trophy, Plus, RefreshCcw, Shield, Edit3, Trash2, Package, Coins } from "lucide-react";

export default function RewardsPage() {
  const { isAdmin } = useAuth();
  const [rewards, setRewards] = useState<AvailableRewardResDTO[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isRefreshing, setIsRefreshing] = useState(false);

  // Create form
  const [showCreate, setShowCreate] = useState(false);
  const [createLoading, setCreateLoading] = useState(false);
  const [form, setForm] = useState({
    name: "",
    description: "",
    pointCost: "",
    stockQuantity: "",
  });

  // Edit form
  const [editReward, setEditReward] = useState<AvailableRewardResDTO | null>(null);
  const [editForm, setEditForm] = useState({
    name: "",
    description: "",
    pointCost: "",
    stockQuantity: "",
    isActive: true,
  });
  const [editLoading, setEditLoading] = useState(false);

  const loadRewards = useCallback(async () => {
    try {
      const res = await pointsApi.getAvailableRewards();
      if (res.isSuccess && res.data) setRewards(res.data);
    } catch {
      toast("error", "Failed to load rewards");
    }
  }, []);

  useEffect(() => {
    setIsLoading(true);
    loadRewards().finally(() => setIsLoading(false));
  }, [loadRewards]);

  const handleRefresh = async () => {
    setIsRefreshing(true);
    await loadRewards();
    setIsRefreshing(false);
    toast("success", "Rewards refreshed");
  };

  const handleCreate = async () => {
    if (!form.name.trim() || !form.description.trim()) {
      toast("error", "Name and description are required");
      return;
    }
    const pointCost = Number(form.pointCost);
    const stockQuantity = Number(form.stockQuantity);
    if (!pointCost || pointCost <= 0) {
      toast("error", "Point cost must be a positive number");
      return;
    }
    if (!stockQuantity || stockQuantity <= 0) {
      toast("error", "Stock quantity must be a positive number");
      return;
    }

    setCreateLoading(true);
    try {
      const res = await pointsApi.createReward({
        name: form.name,
        description: form.description,
        pointCost,
        stockQuantity,
      });
      if (res.isSuccess) {
        toast("success", "Reward created successfully!");
        setForm({ name: "", description: "", pointCost: "", stockQuantity: "" });
        setShowCreate(false);
        await loadRewards();
      } else {
        toast("error", res.message || "Failed to create reward");
      }
    } catch {
      toast("error", "Failed to create reward");
    } finally {
      setCreateLoading(false);
    }
  };

  const handleEdit = (reward: AvailableRewardResDTO) => {
    setEditReward(reward);
    setEditForm({
      name: reward.name,
      description: reward.description,
      pointCost: String(reward.pointCost),
      stockQuantity: String(reward.stockQuantity),
      isActive: reward.isActive,
    });
  };

  const handleUpdate = async () => {
    if (!editReward) return;
    const pointCost = Number(editForm.pointCost);
    const stockQuantity = Number(editForm.stockQuantity);

    if (!editForm.name.trim()) { toast("error", "Name is required"); return; }
    if (!pointCost || pointCost <= 0) { toast("error", "Point cost must be positive"); return; }

    setEditLoading(true);
    try {
      const res = await pointsApi.updateReward(editReward.id, {
        name: editForm.name,
        description: editForm.description,
        pointCost,
        stockQuantity,
        isActive: editForm.isActive,
      });
      if (res.isSuccess) {
        toast("success", "Reward updated successfully!");
        setEditReward(null);
        await loadRewards();
      } else {
        toast("error", res.message || "Failed to update reward");
      }
    } catch {
      toast("error", "Failed to update reward");
    } finally {
      setEditLoading(false);
    }
  };

  const handleDelete = async (reward: AvailableRewardResDTO) => {
    if (!confirm(`Delete reward "${reward.name}"? This cannot be undone.`)) return;
    try {
      const res = await pointsApi.deleteReward(reward.id);
      if (res.isSuccess) {
        toast("success", "Reward deleted");
        await loadRewards();
      } else {
        toast("error", res.message || "Failed to delete reward");
      }
    } catch {
      toast("error", "Failed to delete reward");
    }
  };

  if (!isAdmin) {
    return (
      <AnimatedPage>
        <div className="flex flex-col items-center justify-center h-[60vh] gap-4">
          <Shield size={48} className="text-[var(--text-tertiary)]" />
          <h2 className="text-xl font-semibold text-[var(--text-primary)]">Access Restricted</h2>
          <p className="text-[var(--text-secondary)]">Reward management is available for Admin only.</p>
        </div>
      </AnimatedPage>
    );
  }

  return (
    <AnimatedPage>
      <div className="space-y-6">
        <div className="flex items-center justify-between">
          <div>
            <h2 className="text-2xl font-bold text-[var(--text-primary)]">Manage Rewards</h2>
            <p className="text-sm text-[var(--text-secondary)] mt-1">Create and manage loyalty rewards for customers.</p>
          </div>
          <div className="flex items-center gap-2">
            <Button variant="secondary" size="sm" onClick={handleRefresh} isLoading={isRefreshing} icon={<RefreshCcw size={14} />}>Refresh</Button>
            <Button size="sm" onClick={() => setShowCreate(true)} icon={<Plus size={14} />}>New Reward</Button>
          </div>
        </div>

        {/* Rewards Grid */}
        <Card padding="lg">
          <CardHeader title="All Rewards" subtitle={`${rewards.length} rewards configured`} action={<Trophy size={18} className="text-[var(--text-tertiary)]" />} />

          {isLoading ? (
            <SkeletonTable rows={4} />
          ) : rewards.length === 0 ? (
            <div className="py-16 text-center">
              <Trophy size={48} className="mx-auto mb-3 text-[var(--text-tertiary)] opacity-50" />
              <p className="text-[var(--text-secondary)]">No rewards created yet</p>
              <p className="text-xs text-[var(--text-tertiary)] mt-1">Click &quot;New Reward&quot; to get started</p>
            </div>
          ) : (
            <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
              {rewards.map((reward) => (
                <div key={reward.id} className="rounded-2xl border border-[var(--border-primary)] bg-[var(--bg-secondary)] p-5 hover:border-[var(--border-secondary)] transition-all duration-200 group">
                  <div className="flex items-start justify-between gap-3">
                    <div className="flex-1 min-w-0">
                      <h3 className="text-base font-semibold text-[var(--text-primary)] truncate">{reward.name}</h3>
                      <p className="text-sm text-[var(--text-secondary)] mt-1 line-clamp-2">{reward.description}</p>
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
                      <span className="text-sm text-[var(--text-secondary)]">{reward.stockQuantity} in stock</span>
                    </div>
                  </div>

                  <div className="flex items-center gap-2 mt-4 pt-3 border-t border-[var(--border-primary)]">
                    <Button variant="secondary" size="sm" className="flex-1" onClick={() => handleEdit(reward)} icon={<Edit3 size={14} />}>Edit</Button>
                    <button onClick={() => handleDelete(reward)} className="p-2 rounded-lg text-[var(--text-tertiary)] hover:text-[var(--accent-danger)] hover:bg-[var(--accent-danger-soft)] transition-colors cursor-pointer" title="Delete">
                      <Trash2 size={16} />
                    </button>
                  </div>
                </div>
              ))}
            </div>
          )}
        </Card>

        {/* Create Reward Modal */}
        <Modal isOpen={showCreate} onClose={() => setShowCreate(false)} title="Create New Reward" size="md">
          <div className="space-y-4">
            <Input label="Reward Name" placeholder="e.g. Free Coffee" value={form.name} onChange={(e) => setForm((p) => ({ ...p, name: e.target.value }))} id="reward-name" />
            <div>
              <label className="block text-sm font-medium text-[var(--text-secondary)] mb-1.5">Description</label>
              <textarea
                placeholder="Describe the reward..."
                value={form.description}
                onChange={(e) => setForm((p) => ({ ...p, description: e.target.value }))}
                className="w-full px-4 py-2.5 text-sm rounded-xl bg-[var(--bg-input)] border border-[var(--border-primary)] text-[var(--text-primary)] focus:outline-none focus:ring-2 focus:ring-[var(--accent-primary)] min-h-[80px] resize-none"
                id="reward-description"
              />
            </div>
            <div className="grid grid-cols-2 gap-4">
              <Input label="Point Cost" type="number" placeholder="100" value={form.pointCost} onChange={(e) => setForm((p) => ({ ...p, pointCost: e.target.value }))} id="reward-cost" />
              <Input label="Stock Quantity" type="number" placeholder="50" value={form.stockQuantity} onChange={(e) => setForm((p) => ({ ...p, stockQuantity: e.target.value }))} id="reward-stock" />
            </div>
            <div className="flex gap-3 pt-2">
              <Button variant="secondary" className="flex-1" onClick={() => setShowCreate(false)}>Cancel</Button>
              <Button className="flex-1" onClick={handleCreate} isLoading={createLoading} icon={<Plus size={16} />}>Create Reward</Button>
            </div>
          </div>
        </Modal>

        {/* Edit Reward Modal */}
        <Modal isOpen={!!editReward} onClose={() => setEditReward(null)} title="Edit Reward" size="md">
          <div className="space-y-4">
            <Input label="Reward Name" value={editForm.name} onChange={(e) => setEditForm((p) => ({ ...p, name: e.target.value }))} id="edit-reward-name" />
            <div>
              <label className="block text-sm font-medium text-[var(--text-secondary)] mb-1.5">Description</label>
              <textarea
                value={editForm.description}
                onChange={(e) => setEditForm((p) => ({ ...p, description: e.target.value }))}
                className="w-full px-4 py-2.5 text-sm rounded-xl bg-[var(--bg-input)] border border-[var(--border-primary)] text-[var(--text-primary)] focus:outline-none focus:ring-2 focus:ring-[var(--accent-primary)] min-h-[80px] resize-none"
                id="edit-reward-description"
              />
            </div>
            <div className="grid grid-cols-2 gap-4">
              <Input label="Point Cost" type="number" value={editForm.pointCost} onChange={(e) => setEditForm((p) => ({ ...p, pointCost: e.target.value }))} id="edit-reward-cost" />
              <Input label="Stock Quantity" type="number" value={editForm.stockQuantity} onChange={(e) => setEditForm((p) => ({ ...p, stockQuantity: e.target.value }))} id="edit-reward-stock" />
            </div>
            <div>
              <label className="flex items-center gap-3 cursor-pointer">
                <input
                  type="checkbox"
                  checked={editForm.isActive}
                  onChange={(e) => setEditForm((p) => ({ ...p, isActive: e.target.checked }))}
                  className="w-4 h-4 accent-[var(--accent-primary)] rounded"
                />
                <span className="text-sm text-[var(--text-primary)]">Active (visible to customers)</span>
              </label>
            </div>
            <div className="flex gap-3 pt-2">
              <Button variant="secondary" className="flex-1" onClick={() => setEditReward(null)}>Cancel</Button>
              <Button className="flex-1" onClick={handleUpdate} isLoading={editLoading} icon={<Edit3 size={16} />}>Save Changes</Button>
            </div>
          </div>
        </Modal>
      </div>
    </AnimatedPage>
  );
}
