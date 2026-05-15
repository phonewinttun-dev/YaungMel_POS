"use client";

import { useEffect, useState, useCallback } from "react";
import { inventoryApi, productsApi, searchApi } from "@/lib/api";
import { useAuth } from "@/lib/auth-context";
import type { ProductDTO, PageSettingDTO } from "@/lib/types";
import { Card } from "@/components/ui/Card";
import { Button } from "@/components/ui/Button";
import { Input } from "@/components/ui/Input";
import { Badge } from "@/components/ui/Badge";
import { Modal } from "@/components/ui/Modal";
import { SkeletonTable } from "@/components/ui/Skeleton";
import { AnimatedPage } from "@/components/ui/AnimatedPage";
import { toast } from "@/components/ui/Toast";
import { Warehouse, Plus, Minus, DollarSign, AlertTriangle, Search } from "lucide-react";
import { Pagination } from "@/components/ui/Pagination";

export default function InventoryPage() {
  const { isAdmin } = useAuth();
  const [products, setProducts] = useState<ProductDTO[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [showLowOnly, setShowLowOnly] = useState(false);
  const [adjustModal, setAdjustModal] = useState<{ product: ProductDTO; type: "increase" | "decrease" } | null>(null);
  const [priceModal, setPriceModal] = useState<ProductDTO | null>(null);
  const [quantity, setQuantity] = useState("");
  const [newPrice, setNewPrice] = useState("");
  const [actionLoading, setActionLoading] = useState(false);

  const [searchTerm, setSearchTerm] = useState("");
  const [pageSetting, setPageSetting] = useState<PageSettingDTO>({
    pageNo: 1,
    pageSize: 10,
    pageCount: 0,
  });

  const loadProducts = useCallback(async (page: number = 1) => {
    setIsLoading(true);
    try {
      const isFiltered = !!searchTerm || showLowOnly;
      const res = await (isFiltered 
        ? searchApi.search({
            Name: searchTerm || undefined,
            MaxStockQuantity: showLowOnly ? 5 : undefined,
            PageNumber: page,
            PageSize: pageSetting.pageSize,
            SortBy: "name",
          })
        : productsApi.getPaged(page, pageSetting.pageSize));
        
      if (res.isSuccess && res.data) {
        setProducts(res.data.items);
        setPageSetting(res.data.pageSetting);
      }
    } catch { toast("error", "Failed to load inventory"); }
    finally { setIsLoading(false); }
  }, [searchTerm, showLowOnly, pageSetting.pageSize]);

  useEffect(() => { void loadProducts(1); }, [pageSetting.pageSize]);

  const handleSearch = () => {
    void loadProducts(1);
  };

  const handleAdjust = async () => {
    if (!adjustModal || !quantity || Number(quantity) <= 0) { toast("error", "Enter valid quantity"); return; }
    setActionLoading(true);
    try {
      const fn = adjustModal.type === "increase" ? inventoryApi.increaseStock : inventoryApi.reduceStock;
      const res = await fn({ productId: adjustModal.product.id, quantity: Number(quantity) });
      if (res.isSuccess) { toast("success", `Stock ${adjustModal.type}d`); setAdjustModal(null); setQuantity(""); void loadProducts(); }
      else toast("error", res.message);
    } catch { toast("error", "Failed"); }
    finally { setActionLoading(false); }
  };

  const handlePrice = async () => {
    if (!priceModal || !newPrice || Number(newPrice) <= 0) { toast("error", "Enter valid price"); return; }
    setActionLoading(true);
    try {
      const res = await inventoryApi.updatePrice(priceModal.id, { productId: priceModal.id, newPrice: Number(newPrice) });
      if (res.isSuccess) { toast("success", "Price updated"); setPriceModal(null); setNewPrice(""); void loadProducts(); }
      else toast("error", res.message);
    } catch { toast("error", "Failed"); }
    finally { setActionLoading(false); }
  };

  const stockBadge = (qty: number) => {
    if (qty === 0) return <Badge variant="danger">Out of Stock</Badge>;
    if (qty <= 5) return <Badge variant="warning">Low ({qty})</Badge>;
    return <Badge variant="success">{qty}</Badge>;
  };

  return (
    <AnimatedPage>
      <div className="space-y-6">
        <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
          <div>
            <h2 className="text-2xl font-bold text-[var(--text-primary)]">Inventory</h2>
            <p className="text-sm text-[var(--text-secondary)] mt-1">Track and manage your stock levels</p>
          </div>
          <div className="flex items-center gap-2">
            <Button variant={showLowOnly ? "danger" : "secondary"} onClick={() => setShowLowOnly(!showLowOnly)} icon={<AlertTriangle size={16} />}>
              {showLowOnly ? "Show All" : "Low Stock Only"}
            </Button>
          </div>
        </div>

        <Card padding="sm">
          <div className="flex gap-2 p-2">
            <Input 
              placeholder="Search by product name..." 
              value={searchTerm} 
              onChange={(e) => setSearchTerm(e.target.value)} 
              onKeyDown={(e) => e.key === "Enter" && handleSearch()}
              icon={<Search size={18} />} 
            />
            <Button onClick={handleSearch} variant="secondary">Search</Button>
          </div>
        </Card>

        <Card padding="none">
          {isLoading ? <div className="p-6"><SkeletonTable rows={8} /></div> : products.length === 0 ? (
            <div className="py-16 text-center">
              <Warehouse size={48} className="mx-auto mb-3 text-[var(--text-tertiary)] opacity-50" />
              <p className="text-[var(--text-secondary)]">{showLowOnly ? "No low stock items found" : "No products found"}</p>
            </div>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead>
                  <tr className="border-b border-[var(--border-primary)] bg-[var(--bg-secondary)]">
                    {["#", "Product", "Price", "Stock", "Status", "Actions"].map((h) => (
                      <th key={h} className="text-left py-3 px-4 text-xs font-semibold text-[var(--text-tertiary)] uppercase tracking-wider last:text-right">{h}</th>
                    ))}
                  </tr>
                </thead>
                <tbody>
                  {products.map((p, index) => (
                    <tr key={p.id} className="border-b border-[var(--border-primary)] last:border-0 hover:bg-[var(--bg-hover)] transition-colors">
                      <td className="py-3 px-4 text-sm font-medium text-[var(--text-tertiary)]">{(pageSetting.pageNo - 1) * pageSetting.pageSize + index + 1}</td>
                      <td className="py-3 px-4 text-sm font-medium text-[var(--text-primary)]">{p.name}</td>
                      <td className="py-3 px-4 text-sm font-mono text-[var(--text-primary)]">{p.priceFormatted} MMK</td>
                      <td className="py-3 px-4">{stockBadge(p.stockQuantity)}</td>
                      <td className="py-3 px-4"><Badge variant={p.isActive ? "success" : "danger"}>{p.isActive ? "Active" : "Inactive"}</Badge></td>
                      <td className="py-3 px-4 text-right">
                        {isAdmin && (
                          <div className="flex items-center justify-end gap-1">
                            <button onClick={() => { setAdjustModal({ product: p, type: "increase" }); setQuantity(""); }} className="p-2 rounded-lg text-[var(--text-tertiary)] hover:text-[var(--accent-success)] hover:bg-[var(--accent-success-soft)] transition-colors cursor-pointer" title="Increase stock"><Plus size={16} /></button>
                            <button onClick={() => { setAdjustModal({ product: p, type: "decrease" }); setQuantity(""); }} className="p-2 rounded-lg text-[var(--text-tertiary)] hover:text-[var(--accent-warning)] hover:bg-[var(--accent-warning-soft)] transition-colors cursor-pointer" title="Decrease stock"><Minus size={16} /></button>
                            <button onClick={() => { setPriceModal(p); setNewPrice(p.price.toString()); }} className="p-2 rounded-lg text-[var(--text-tertiary)] hover:text-[var(--accent-primary)] hover:bg-[var(--accent-primary-soft)] transition-colors cursor-pointer" title="Update price"><DollarSign size={16} /></button>
                          </div>
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </Card>

        {pageSetting.pageCount > 1 && (
          <Pagination
            currentPage={pageSetting.pageNo}
            totalPages={pageSetting.pageCount}
            onPageChange={(page) => void loadProducts(page)}
          />
        )}

        <Modal isOpen={!!adjustModal} onClose={() => setAdjustModal(null)} title={`${adjustModal?.type === "increase" ? "Increase" : "Decrease"} Stock — ${adjustModal?.product.name}`} size="sm">
          <div className="space-y-4">
            <p className="text-sm text-[var(--text-secondary)]">Current stock: <strong>{adjustModal?.product.stockQuantity}</strong></p>
            <Input label="Quantity" type="number" value={quantity} onChange={(e) => setQuantity(e.target.value)} placeholder="Enter quantity" />
            <div className="flex justify-end gap-3">
              <Button variant="secondary" onClick={() => setAdjustModal(null)}>Cancel</Button>
              <Button variant={adjustModal?.type === "increase" ? "success" : "danger"} onClick={handleAdjust} isLoading={actionLoading}>
                {adjustModal?.type === "increase" ? "Increase" : "Decrease"}
              </Button>
            </div>
          </div>
        </Modal>

        <Modal isOpen={!!priceModal} onClose={() => setPriceModal(null)} title={`Update Price — ${priceModal?.name}`} size="sm">
          <div className="space-y-4">
            <p className="text-sm text-[var(--text-secondary)]">Current price: <strong>{priceModal?.priceFormatted} MMK</strong></p>
            <Input label="New Price" type="number" value={newPrice} onChange={(e) => setNewPrice(e.target.value)} placeholder="0.00" />
            <div className="flex justify-end gap-3">
              <Button variant="secondary" onClick={() => setPriceModal(null)}>Cancel</Button>
              <Button onClick={handlePrice} isLoading={actionLoading}>Update Price</Button>
            </div>
          </div>
        </Modal>
      </div>
    </AnimatedPage>
  );
}
