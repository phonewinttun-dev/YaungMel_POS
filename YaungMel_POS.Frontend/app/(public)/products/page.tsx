"use client";

import { AnimatedPage } from "@/components/ui/AnimatedPage";
import { Badge } from "@/components/ui/Badge";
import { Button } from "@/components/ui/Button";
import { Card } from "@/components/ui/Card";
import { Input } from "@/components/ui/Input";
import { Modal } from "@/components/ui/Modal";
import { Pagination } from "@/components/ui/Pagination";
import { SkeletonTable } from "@/components/ui/Skeleton";
import { toast } from "@/components/ui/Toast";
import { categoriesApi, productsApi } from "@/lib/api";
import { useAuth } from "@/lib/auth-context";
import type { CategoryDTO, ProductDTO } from "@/lib/types";
import { Edit2, Filter, Package, Plus, Search, Trash2, Upload } from "lucide-react";
import { useCallback, useEffect, useState } from "react";

export default function ProductsPage() {
  const { isAdmin } = useAuth();
  const [products, setProducts] = useState<ProductDTO[]>([]);
  const [categories, setCategories] = useState<CategoryDTO[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState("");
  const [filterCategory, setFilterCategory] = useState<number | null>(null);
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [editProduct, setEditProduct] = useState<ProductDTO | null>(null);
  const [deleteId, setDeleteId] = useState<number | null>(null);
  const [form, setForm] = useState({ name: "", description: "", price: "", stockQuantity: "", categoryId: "" });
  const [photoFile, setPhotoFile] = useState<File | null>(null);
  const [formLoading, setFormLoading] = useState(false);

  const [currentPage, setCurrentPage] = useState(1);
  const pageSize = 10;

  const loadData = useCallback(async () => {
    setIsLoading(true);
    try {
      const [prodRes, catRes] = await Promise.all([productsApi.getAll(), categoriesApi.getAll()]);
      if (prodRes.isSuccess && prodRes.data) {
        const sortedProducts = prodRes.data
          .filter((p) => !p.deleteFlag)
          .sort((a, b) => a.name.localeCompare(b.name));
        setProducts(sortedProducts);
      }
      if (catRes.isSuccess && catRes.data) {
        const sortedCategories = [...catRes.data].sort((a, b) => a.name.localeCompare(b.name));
        setCategories(sortedCategories);
      }
    } catch { toast("error", "Failed to load products"); }
    finally { setIsLoading(false); }
  }, []);

  useEffect(() => { void loadData(); }, [loadData]);

  const getCategoryName = (id: number) => categories.find((c) => c.id === id)?.name || "Unknown";

  const filtered = products.filter((p) => {
    const matchesSearch = !searchTerm || p.name.toLowerCase().includes(searchTerm.toLowerCase()) || p.description?.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesCategory = !filterCategory || p.categoryId === filterCategory;
    return matchesSearch && matchesCategory;
  });

  const totalPages = Math.ceil(filtered.length / pageSize);
  const paginated = filtered.slice((currentPage - 1) * pageSize, currentPage * pageSize);

  useEffect(() => {
    setCurrentPage(1);
  }, [searchTerm, filterCategory]);

  const openCreate = () => {
    setForm({ name: "", description: "", price: "", stockQuantity: "", categoryId: categories[0]?.id?.toString() || "" });
    setPhotoFile(null);
    setEditProduct(null);
    setShowCreateModal(true);
  };

  const openEdit = (p: ProductDTO) => {
    setForm({ name: p.name, description: p.description || "", price: p.price.toString(), stockQuantity: p.stockQuantity.toString(), categoryId: p.categoryId.toString() });
    setPhotoFile(null);
    setEditProduct(p);
    setShowCreateModal(true);
  };

  const handleSave = async () => {
    if (!form.name.trim()) { toast("error", "Name is required"); return; }
    if (!form.price || isNaN(Number(form.price))) { toast("error", "Valid price required"); return; }
    if (!form.categoryId) { toast("error", "Select a category"); return; }
    setFormLoading(true);
    try {
      if (editProduct) {
        const updatePayload = {
          name: form.name,
          description: form.description || undefined,
          price: Number(form.price),
          stockQuantity: Number(form.stockQuantity),
          categoryId: Number(form.categoryId),
          version: editProduct.version,
        };
        const res = photoFile
          ? await (() => {
            const payload = new FormData();
            payload.append("Name", form.name);
            payload.append("Description", form.description || "");
            payload.append("Price", Number(form.price).toString());
            payload.append("StockQuantity", Number(form.stockQuantity).toString());
            payload.append("CategoryId", Number(form.categoryId).toString());
            payload.append("Version", String(editProduct.version ?? 0));
            payload.append("photoFile", photoFile);
            return productsApi.updateWithPhoto(editProduct.id, payload);
          })()
          : await productsApi.update(editProduct.id, updatePayload);
        if (res.isSuccess) { toast("success", "Product updated"); setShowCreateModal(false); void loadData(); }
        else toast("error", res.message);
      } else {
        let res;
        if (photoFile) {
          const payload = new FormData();
          payload.append("Name", form.name);
          payload.append("Description", form.description || "");
          payload.append("Price", Number(form.price).toString());
          payload.append("StockQuantity", (Number(form.stockQuantity) || 0).toString());
          payload.append("CategoryId", Number(form.categoryId).toString());
          payload.append("photoFile", photoFile);
          res = await productsApi.createWithPhoto(payload);
        } else {
          res = await productsApi.create({ name: form.name, description: form.description || undefined, price: Number(form.price), stockQuantity: Number(form.stockQuantity) || 0, categoryId: Number(form.categoryId) });
        }
        if (res.isSuccess) { toast("success", "Product created"); setShowCreateModal(false); void loadData(); }
        else toast("error", res.message);
      }
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message || "Operation failed";
      toast("error", msg);
    } finally { setFormLoading(false); }
  };

  const handleDelete = async () => {
    if (!deleteId) return;
    try {
      const res = await productsApi.delete(deleteId);
      if (res.isSuccess) { toast("success", "Product deleted"); setDeleteId(null); void loadData(); }
      else toast("error", res.message);
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message || "Delete failed";
      toast("error", msg);
    }
  };

  const stockBadge = (qty: number) => {
    if (qty === 0) return <Badge variant="danger">Out of Stock</Badge>;
    if (qty <= 5) return <Badge variant="warning">Low ({qty})</Badge>;
    return <Badge variant="success">{qty} in stock</Badge>;
  };

  return (
    <AnimatedPage>
      <div className="space-y-6">
        <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
          <div>
            <h2 className="text-2xl font-bold text-[var(--text-primary)]">Products</h2>
            <p className="text-sm text-[var(--text-secondary)] mt-1">{filtered.length} products</p>
          </div>
          <Button onClick={openCreate} icon={<Plus size={18} />}>Add Product</Button>
        </div>

        <Card padding="sm">
          <div className="flex flex-col sm:flex-row gap-3 p-2">
            <div className="flex-1">
              <Input placeholder="Search products..." value={searchTerm} onChange={(e) => setSearchTerm(e.target.value)} icon={<Search size={18} />} />
            </div>
            <div className="flex items-center gap-2">
              <Filter size={16} className="text-[var(--text-tertiary)] shrink-0" />
              <select value={filterCategory || ""} onChange={(e) => setFilterCategory(e.target.value ? Number(e.target.value) : null)} className="px-3 py-2.5 text-sm rounded-xl bg-[var(--bg-input)] border border-[var(--border-primary)] text-[var(--text-primary)] focus:outline-none focus:ring-2 focus:ring-[var(--accent-primary)]">
                <option value="">All Categories</option>
                {categories.map((c) => (<option key={c.id} value={c.id}>{c.name}</option>))}
              </select>
            </div>
          </div>
        </Card>

        <Card padding="none">
          {isLoading ? (<div className="p-6"><SkeletonTable rows={6} /></div>) : filtered.length === 0 ? (
            <div className="py-16 text-center">
              <Package size={48} className="mx-auto mb-3 text-[var(--text-tertiary)] opacity-50" />
              <p className="text-[var(--text-secondary)]">No products found</p>
            </div>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead>
                  <tr className="border-b border-[var(--border-primary)] bg-[var(--bg-secondary)]">
                    {["Product", "Category", "Price", "Stock", "Status", "Actions"].map((h) => (
                      <th key={h} className="text-left py-3 px-4 text-xs font-semibold text-[var(--text-tertiary)] uppercase tracking-wider last:text-right">{h}</th>
                    ))}
                  </tr>
                </thead>
                <tbody>
                  {paginated.map((p) => (
                    <tr key={p.id} className="border-b border-[var(--border-primary)] last:border-0 hover:bg-[var(--bg-hover)] transition-colors">
                      <td className="py-3 px-4">
                        <div className="flex items-center gap-3">
                          {p.imageUrl ? (
                            <img
                              src={p.imageUrl}
                              alt={p.name}
                              className="h-11 w-11 rounded-lg object-cover border border-[var(--border-primary)] shrink-0"
                              loading="lazy"
                            />
                          ) : (
                            <div className="h-11 w-11 rounded-lg border border-[var(--border-primary)] bg-[var(--bg-secondary)] shrink-0" />
                          )}
                          <div>
                            <p className="text-sm font-medium text-[var(--text-primary)]">{p.name}</p>
                            {p.description && <p className="text-xs text-[var(--text-tertiary)] mt-0.5 truncate max-w-[200px]">{p.description}</p>}
                          </div>
                        </div>
                      </td>
                      <td className="py-3 px-4"><Badge variant="info">{getCategoryName(p.categoryId)}</Badge></td>
                      <td className="py-3 px-4"><span className="text-sm font-mono font-medium text-[var(--text-primary)]">{p.priceFormatted} MMK</span></td>
                      <td className="py-3 px-4">{stockBadge(p.stockQuantity)}</td>
                      <td className="py-3 px-4"><Badge variant={p.isActive ? "success" : "danger"}>{p.isActive ? "Active" : "Inactive"}</Badge></td>
                      <td className="py-3 px-4 text-right">
                        <div className="flex items-center justify-end gap-1">
                          <button onClick={() => openEdit(p)} className="p-2 rounded-lg text-[var(--text-tertiary)] hover:text-[var(--accent-primary)] hover:bg-[var(--accent-primary-soft)] transition-colors cursor-pointer"><Edit2 size={16} /></button>
                          {isAdmin && (<button onClick={() => setDeleteId(p.id)} className="p-2 rounded-lg text-[var(--text-tertiary)] hover:text-[var(--accent-danger)] hover:bg-[var(--accent-danger-soft)] transition-colors cursor-pointer"><Trash2 size={16} /></button>)}
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </Card>

        {filtered.length > pageSize && (
          <Pagination
            currentPage={currentPage}
            totalPages={totalPages}
            onPageChange={setCurrentPage}
          />
        )}

        <Modal isOpen={showCreateModal} onClose={() => setShowCreateModal(false)} title={editProduct ? "Edit Product" : "New Product"} size="md">
          <div className="space-y-4">
            <Input label="Name" value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} placeholder="Product name" />
            <Input label="Description" value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} placeholder="Optional description" />
            <div className="grid grid-cols-2 gap-4">
              <Input label="Price" type="number" value={form.price} onChange={(e) => setForm({ ...form, price: e.target.value })} placeholder="0.00" />
              <Input label="Stock Quantity" type="number" value={form.stockQuantity} onChange={(e) => setForm({ ...form, stockQuantity: e.target.value })} placeholder="0" />
            </div>
            <div>
              <label className="block text-sm font-medium text-[var(--text-secondary)] mb-1.5">Category</label>
              <select value={form.categoryId} onChange={(e) => setForm({ ...form, categoryId: e.target.value })} className="w-full px-4 py-2.5 text-sm rounded-xl bg-[var(--bg-input)] border border-[var(--border-primary)] text-[var(--text-primary)] focus:outline-none focus:ring-2 focus:ring-[var(--accent-primary)]">
                <option value="">Select category</option>
                {categories.map((c) => (<option key={c.id} value={c.id}>{c.name}</option>))}
              </select>
            </div>
            <div className="rounded-xl border border-dashed border-[var(--border-primary)] bg-[var(--bg-secondary)] p-4">
              <div className="flex items-center justify-between gap-3">
                <div>
                  <p className="text-sm font-medium text-[var(--text-primary)]">
                    {editProduct ? "Product Image" : "Product Photo"}
                  </p>
                  <p className="text-xs text-[var(--text-tertiary)] mt-0.5">
                    {editProduct ? "Select a new image to update current product image." : "Upload an optional image (jpg, png, webp)"}
                  </p>
                  {photoFile && <p className="text-xs text-[var(--accent-primary)] mt-1">{photoFile.name}</p>}
                </div>
                <label className="cursor-pointer">
                  <input
                    type="file"
                    accept="image/*"
                    className="hidden"
                    onChange={(e) => setPhotoFile(e.target.files?.[0] || null)}
                  />
                  <span className="inline-flex items-center gap-2 px-3 py-2 rounded-lg text-sm font-medium bg-[var(--accent-primary-soft)] text-[var(--accent-primary)] hover:opacity-90 transition">
                    <Upload size={16} />
                    {editProduct ? "Update Image" : "Choose Photo"}
                  </span>
                </label>
              </div>
            </div>
            <div className="flex justify-end gap-3 pt-2">
              <Button variant="secondary" onClick={() => { setShowCreateModal(false); setPhotoFile(null); }}>Cancel</Button>
              <Button onClick={handleSave} isLoading={formLoading}>{editProduct ? "Update" : "Create"}</Button>
            </div>
          </div>
        </Modal>

        <Modal isOpen={!!deleteId} onClose={() => setDeleteId(null)} title="Delete Product" size="sm">
          <p className="text-sm text-[var(--text-secondary)] mb-6">Are you sure you want to delete this product? This action cannot be undone.</p>
          <div className="flex justify-end gap-3">
            <Button variant="secondary" onClick={() => setDeleteId(null)}>Cancel</Button>
            <Button variant="danger" onClick={handleDelete}>Delete</Button>
          </div>
        </Modal>
      </div>
    </AnimatedPage>
  );
}
