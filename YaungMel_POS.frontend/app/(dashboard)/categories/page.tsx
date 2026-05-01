"use client";

import { useEffect, useState, useCallback } from "react";
import { categoriesApi } from "@/lib/api";
import type { CategoryDTO } from "@/lib/types";
import { Card } from "@/components/ui/Card";
import { Button } from "@/components/ui/Button";
import { Input } from "@/components/ui/Input";
import { Modal } from "@/components/ui/Modal";
import { SkeletonCard } from "@/components/ui/Skeleton";
import { AnimatedPage } from "@/components/ui/AnimatedPage";
import { toast } from "@/components/ui/Toast";
import { useAuth } from "@/lib/auth-context";
import { Plus, Search, Edit2, Trash2, Tags, FolderOpen } from "lucide-react";

export default function CategoriesPage() {
  const { isAdmin } = useAuth();
  const [categories, setCategories] = useState<CategoryDTO[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState("");
  const [showModal, setShowModal] = useState(false);
  const [editCat, setEditCat] = useState<CategoryDTO | null>(null);
  const [deleteId, setDeleteId] = useState<number | null>(null);
  const [form, setForm] = useState({ name: "", description: "" });
  const [formLoading, setFormLoading] = useState(false);

  const loadCategories = useCallback(async () => {
    setIsLoading(true);
    try {
      const res = await categoriesApi.getAll();
      if (res.isSuccess && res.data) setCategories(res.data);
    } catch { toast("error", "Failed to load categories"); }
    finally { setIsLoading(false); }
  }, []);

  useEffect(() => { void loadCategories(); }, [loadCategories]);

  const filtered = categories.filter((c) => !searchTerm || c.name.toLowerCase().includes(searchTerm.toLowerCase()));

  const openCreate = () => { setForm({ name: "", description: "" }); setEditCat(null); setShowModal(true); };
  const openEdit = (c: CategoryDTO) => { setForm({ name: c.name, description: c.description || "" }); setEditCat(c); setShowModal(true); };

  const handleSave = async () => {
    if (!form.name.trim()) { toast("error", "Name is required"); return; }
    setFormLoading(true);
    try {
      const payload = { name: form.name, description: form.description || undefined };
      const res = editCat ? await categoriesApi.update(editCat.id, payload) : await categoriesApi.create(payload);
      if (res.isSuccess) { toast("success", editCat ? "Updated" : "Created"); setShowModal(false); void loadCategories(); }
      else toast("error", res.message);
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message || "Operation failed";
      toast("error", msg);
    } finally { setFormLoading(false); }
  };

  const handleDelete = async () => {
    if (!deleteId) return;
    try {
      const res = await categoriesApi.delete(deleteId);
      if (res.isSuccess) { toast("success", "Deleted"); setDeleteId(null); void loadCategories(); }
      else toast("error", res.message);
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message || "Delete failed";
      toast("error", msg);
    }
  };

  const gradients = ["from-blue-500 to-cyan-500", "from-violet-500 to-purple-500", "from-emerald-500 to-teal-500", "from-amber-500 to-orange-500", "from-pink-500 to-rose-500", "from-indigo-500 to-blue-500"];

  return (
    <AnimatedPage>
      <div className="space-y-6">
        <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
          <div>
            <h2 className="text-2xl font-bold text-[var(--text-primary)]">Categories</h2>
            <p className="text-sm text-[var(--text-secondary)] mt-1">{categories.length} categories</p>
          </div>
          <Button onClick={openCreate} icon={<Plus size={18} />}>Add Category</Button>
        </div>

        <div className="max-w-md">
          <Input placeholder="Search categories..." value={searchTerm} onChange={(e) => setSearchTerm(e.target.value)} icon={<Search size={18} />} />
        </div>

        {isLoading ? (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
            {Array.from({ length: 6 }).map((_, i) => <SkeletonCard key={i} />)}
          </div>
        ) : filtered.length === 0 ? (
          <div className="py-16 text-center">
            <FolderOpen size={48} className="mx-auto mb-3 text-[var(--text-tertiary)] opacity-50" />
            <p className="text-[var(--text-secondary)]">No categories found</p>
          </div>
        ) : (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
            {filtered.map((c, i) => (
              <Card key={c.id} hover>
                <div className="flex items-start justify-between">
                  <div className="flex items-center gap-3">
                    <div className={`w-10 h-10 rounded-xl bg-gradient-to-br ${gradients[i % gradients.length]} flex items-center justify-center shadow-md`}>
                      <Tags size={20} className="text-white" />
                    </div>
                    <div>
                      <h3 className="text-base font-semibold text-[var(--text-primary)]">{c.name}</h3>
                      <p className="text-xs text-[var(--text-tertiary)] mt-0.5 line-clamp-1">{c.description || "No description"}</p>
                    </div>
                  </div>
                  <div className="flex items-center gap-1">
                    <button onClick={() => openEdit(c)} className="p-2 rounded-lg text-[var(--text-tertiary)] hover:text-[var(--accent-primary)] hover:bg-[var(--accent-primary-soft)] transition-colors cursor-pointer"><Edit2 size={15} /></button>
                    {isAdmin && <button onClick={() => setDeleteId(c.id)} className="p-2 rounded-lg text-[var(--text-tertiary)] hover:text-[var(--accent-danger)] hover:bg-[var(--accent-danger-soft)] transition-colors cursor-pointer"><Trash2 size={15} /></button>}
                  </div>
                </div>
              </Card>
            ))}
          </div>
        )}

        <Modal isOpen={showModal} onClose={() => setShowModal(false)} title={editCat ? "Edit Category" : "New Category"} size="sm">
          <div className="space-y-4">
            <Input label="Name" value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} placeholder="Category name" />
            <Input label="Description" value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} placeholder="Optional" />
            <div className="flex justify-end gap-3 pt-2">
              <Button variant="secondary" onClick={() => setShowModal(false)}>Cancel</Button>
              <Button onClick={handleSave} isLoading={formLoading}>{editCat ? "Update" : "Create"}</Button>
            </div>
          </div>
        </Modal>

        <Modal isOpen={!!deleteId} onClose={() => setDeleteId(null)} title="Delete Category" size="sm">
          <p className="text-sm text-[var(--text-secondary)] mb-6">Categories with products cannot be deleted.</p>
          <div className="flex justify-end gap-3">
            <Button variant="secondary" onClick={() => setDeleteId(null)}>Cancel</Button>
            <Button variant="danger" onClick={handleDelete}>Delete</Button>
          </div>
        </Modal>
      </div>
    </AnimatedPage>
  );
}
