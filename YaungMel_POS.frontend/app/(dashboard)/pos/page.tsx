"use client";

import { useEffect, useState, useCallback } from "react";
import { useRouter } from "next/navigation";
import { productsApi, categoriesApi, salesApi, pointsApi } from "@/lib/api";
import type { ProductDTO, CategoryDTO, CartItem } from "@/lib/types";
import { Card } from "@/components/ui/Card";
import { Button } from "@/components/ui/Button";
import { Input } from "@/components/ui/Input";
import { Badge } from "@/components/ui/Badge";
import { Modal } from "@/components/ui/Modal";
import { AnimatedPage } from "@/components/ui/AnimatedPage";
import { toast } from "@/components/ui/Toast";
import { Search, Plus, Minus, ShoppingCart, CheckCircle, X, Gift, UserCheck } from "lucide-react";

export default function POSPage() {

  const router = useRouter();
  const [products, setProducts] = useState<ProductDTO[]>([]);
  const [categories, setCategories] = useState<CategoryDTO[]>([]);
  const [cart, setCart] = useState<CartItem[]>([]);
  const [searchTerm, setSearchTerm] = useState("");
  const [filterCat, setFilterCat] = useState<number | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [checkoutLoading, setCheckoutLoading] = useState(false);
  const [showReceipt, setShowReceipt] = useState<{ voucherCode: string; total: number } | null>(null);

  // Loyalty Workflow State
  const [pendingReceipt, setPendingReceipt] = useState<{ voucherCode: string; total: number } | null>(null);
  const [showTakePoints, setShowTakePoints] = useState(false);
  const [showAccountCheck, setShowAccountCheck] = useState(false);
  const [showBindPoints, setShowBindPoints] = useState(false);
  const [externalId, setExternalId] = useState("");
  const [pointLoading, setPointLoading] = useState(false);

  const loadProducts = useCallback(async () => {
    setIsLoading(true);
    try {
      const [pRes, cRes] = await Promise.all([productsApi.getAvailable(), categoriesApi.getAll()]);
      if (pRes.isSuccess && pRes.data) setProducts(pRes.data);
      if (cRes.isSuccess && cRes.data) setCategories(cRes.data);
    } catch { toast("error", "Failed to load products"); }
    finally { setIsLoading(false); }
  }, []);

  useEffect(() => { void loadProducts(); }, [loadProducts]);

  const filtered = products.filter((p) => {
    const matchSearch = !searchTerm || p.name.toLowerCase().includes(searchTerm.toLowerCase());
    const matchCat = !filterCat || p.categoryId === filterCat;
    return matchSearch && matchCat;
  });

  const addToCart = (product: ProductDTO) => {
    setCart((prev) => {
      const existing = prev.find((c) => c.product.id === product.id);
      if (existing) {
        if (existing.quantity >= product.stockQuantity) { toast("warning", "Max stock reached"); return prev; }
        return prev.map((c) => c.product.id === product.id ? { ...c, quantity: c.quantity + 1 } : c);
      }
      return [...prev, { product, quantity: 1 }];
    });
  };

  const updateQty = (productId: number, delta: number) => {
    setCart((prev) => prev.map((c) => {
      if (c.product.id !== productId) return c;
      const newQty = c.quantity + delta;
      if (newQty <= 0) return c;
      if (newQty > c.product.stockQuantity) { toast("warning", "Max stock reached"); return c; }
      return { ...c, quantity: newQty };
    }));
  };

  const removeFromCart = (productId: number) => setCart((prev) => prev.filter((c) => c.product.id !== productId));
  const cartTotal = cart.reduce((sum, c) => sum + c.product.price * c.quantity, 0);

  const handleCheckout = async () => {
    if (cart.length === 0) { toast("warning", "Cart is empty"); return; }
    setCheckoutLoading(true);
    try {
      const res = await salesApi.create({ items: cart.map((c) => ({ productId: c.product.id, quantity: c.quantity })) });
      if (res.isSuccess && res.data) {
        setPendingReceipt({ voucherCode: res.data.voucherCode, total: res.data.totalPrice });
        setCart([]);
        void loadProducts();
        setShowTakePoints(true);
      } else toast("error", res.message);
    } catch { toast("error", "Checkout failed"); }
    finally { setCheckoutLoading(false); }
  };

  const handleSkipPoints = () => {
    setShowReceipt(pendingReceipt);
    setPendingReceipt(null);
    setShowTakePoints(false);
  };

  const handleEarnPoints = async () => {
    if (!externalId.trim()) { toast("error", "Account ID is required"); return; }
    if (!pendingReceipt) return;

    setPointLoading(true);
    try {
      const res = await pointsApi.earnPoints({
        externalUserId: externalId,
        eventKey: "PURCHASE",
        eventValue: pendingReceipt.total,
        referenceId: pendingReceipt.voucherCode,
        description: `Purchase reward for voucher ${pendingReceipt.voucherCode}`,
        mobile: "", // Optional in backend if externalId is used
        email: "",
      });

      if (res.isSuccess) {
        toast("success", "Loyalty points awarded!");
        setShowReceipt(pendingReceipt);
        setPendingReceipt(null);
        setShowBindPoints(false);
        setExternalId("");
      } else {
        toast("error", res.message);
      }
    } catch {
      toast("error", "Failed to award points");
    } finally {
      setPointLoading(false);
    }
  };

  return (
    <AnimatedPage>
      <div className="flex flex-col lg:flex-row gap-6 h-[calc(100vh-8rem)]">
        {/* Product Grid */}
        <div className="flex-1 flex flex-col min-w-0">
          <div className="flex flex-col sm:flex-row gap-3 mb-4">
            <div className="flex-1"><Input placeholder="Search products..." value={searchTerm} onChange={(e) => setSearchTerm(e.target.value)} icon={<Search size={18} />} /></div>
            <select value={filterCat || ""} onChange={(e) => setFilterCat(e.target.value ? Number(e.target.value) : null)} className="px-3 py-2.5 text-sm rounded-xl bg-[var(--bg-input)] border border-[var(--border-primary)] text-[var(--text-primary)] focus:outline-none focus:ring-2 focus:ring-[var(--accent-primary)]">
              <option value="">All Categories</option>
              {categories.map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
            </select>
          </div>

          <div className="flex-1 overflow-y-auto grid grid-cols-2 sm:grid-cols-3 xl:grid-cols-4 gap-3 content-start">
            {isLoading ? Array.from({ length: 8 }).map((_, i) => (
              <div key={i} className="h-32 rounded-2xl bg-[var(--bg-tertiary)] animate-[shimmer_1.5s_infinite] bg-[length:200%_100%] bg-gradient-to-r from-[var(--bg-tertiary)] via-[var(--bg-hover)] to-[var(--bg-tertiary)]" />
            )) : filtered.map((p) => {
              const inCart = cart.find((c) => c.product.id === p.id);
              return (
                <button key={p.id} onClick={() => addToCart(p)} className={`text-left p-4 rounded-2xl border transition-all duration-200 hover:shadow-md cursor-pointer ${inCart ? "bg-[var(--accent-primary-soft)] border-[var(--accent-primary)]" : "bg-[var(--bg-card)] border-[var(--border-primary)] hover:border-[var(--border-secondary)]"}`}>
                  <p className="text-sm font-semibold text-[var(--text-primary)] truncate">{p.name}</p>
                  <p className="text-lg font-bold text-[var(--accent-primary)] mt-1">MMK{p.price.toFixed(2)}</p>
                  <div className="flex items-center justify-between mt-2">
                    <span className="text-xs text-[var(--text-tertiary)]">{p.stockQuantity} left</span>
                    {inCart && <Badge variant="primary">{inCart.quantity}x</Badge>}
                  </div>
                </button>
              );
            })}
          </div>
        </div>

        {/* Cart Panel */}
        <Card className="lg:w-96 flex flex-col shrink-0" padding="none">
          <div className="p-4 border-b border-[var(--border-primary)] flex items-center gap-2">
            <ShoppingCart size={20} className="text-[var(--accent-primary)]" />
            <h3 className="text-base font-semibold text-[var(--text-primary)]">Cart</h3>
            <Badge variant="primary" className="ml-auto">{cart.length}</Badge>
          </div>

          <div className="flex-1 overflow-y-auto p-4 space-y-3 min-h-[200px] max-h-[50vh] lg:max-h-none">
            {cart.length === 0 ? (
              <div className="h-full flex items-center justify-center text-[var(--text-tertiary)]">
                <p className="text-sm">Add products to start</p>
              </div>
            ) : cart.map((c) => (
              <div key={c.product.id} className="flex items-center gap-3 p-3 rounded-xl bg-[var(--bg-secondary)] border border-[var(--border-primary)]">
                <div className="flex-1 min-w-0">
                  <p className="text-sm font-medium text-[var(--text-primary)] truncate">{c.product.name}</p>
                  <p className="text-xs text-[var(--text-tertiary)]">MMK{c.product.price.toFixed(2)} each</p>
                </div>
                <div className="flex items-center gap-1.5">
                  <button onClick={() => updateQty(c.product.id, -1)} className="w-7 h-7 rounded-lg bg-[var(--bg-hover)] flex items-center justify-center text-[var(--text-secondary)] hover:bg-[var(--bg-active)] transition-colors cursor-pointer"><Minus size={14} /></button>
                  <span className="w-8 text-center text-sm font-semibold text-[var(--text-primary)]">{c.quantity}</span>
                  <button onClick={() => updateQty(c.product.id, 1)} className="w-7 h-7 rounded-lg bg-[var(--bg-hover)] flex items-center justify-center text-[var(--text-secondary)] hover:bg-[var(--bg-active)] transition-colors cursor-pointer"><Plus size={14} /></button>
                </div>
                <div className="text-right w-16">
                  <p className="text-sm font-semibold text-[var(--text-primary)]">MMK{(c.product.price * c.quantity).toFixed(2)}</p>
                </div>
                <button onClick={() => removeFromCart(c.product.id)} className="p-1 text-[var(--text-tertiary)] hover:text-[var(--accent-danger)] transition-colors cursor-pointer"><X size={14} /></button>
              </div>
            ))}
          </div>

          <div className="p-4 border-t border-[var(--border-primary)] space-y-3">
            <div className="flex items-center justify-between">
              <span className="text-sm text-[var(--text-secondary)]">Total</span>
              <span className="text-xl font-bold text-[var(--text-primary)]">MMK{cartTotal.toFixed(2)}</span>
            </div>
            <Button className="w-full" size="lg" onClick={handleCheckout} isLoading={checkoutLoading} disabled={cart.length === 0} icon={<CheckCircle size={18} />}>
              Checkout
            </Button>
          </div>
        </Card>

        {/* Receipt Modal */}
        <Modal isOpen={!!showReceipt} onClose={() => setShowReceipt(null)} title="Sale Complete!" size="sm">
          {showReceipt && (
            <div className="text-center space-y-4">
              <div className="w-16 h-16 rounded-full bg-[var(--accent-success-soft)] flex items-center justify-center mx-auto">
                <CheckCircle size={32} className="text-[var(--accent-success)]" />
              </div>
              <div>
                <p className="text-sm text-[var(--text-secondary)]">Voucher Code</p>
                <p className="text-lg font-bold font-mono text-[var(--accent-primary)] mt-1">{showReceipt.voucherCode}</p>
              </div>
              <div>
                <p className="text-sm text-[var(--text-secondary)]">Total Amount</p>
                <p className="text-2xl font-bold text-[var(--text-primary)]">MMK{showReceipt.total.toFixed(2)}</p>
              </div>
              <Button className="w-full" onClick={() => setShowReceipt(null)}>Done</Button>
            </div>
          )}
        </Modal>

        {/* Loyalty Step 1: Take Points */}
        <Modal isOpen={showTakePoints} onClose={handleSkipPoints} title="Loyalty Points" size="sm">
          <div className="text-center space-y-6">
            <div className="w-16 h-16 rounded-full bg-[var(--accent-warning-soft)] flex items-center justify-center mx-auto">
              <Gift size={32} className="text-[var(--accent-warning)]" />
            </div>
            <p className="text-[var(--text-primary)] font-medium">Would you like to take loyalty points for this purchase?</p>
            <div className="flex gap-3">
              <Button variant="secondary" className="flex-1" onClick={handleSkipPoints}>No</Button>
              <Button className="flex-1" onClick={() => { setShowTakePoints(false); setShowAccountCheck(true); }}>Yes</Button>
            </div>
          </div>
        </Modal>

        {/* Loyalty Step 2: Account Check */}
        <Modal isOpen={showAccountCheck} onClose={handleSkipPoints} title="Customer Account" size="sm">
          <div className="text-center space-y-6">
            <div className="w-16 h-16 rounded-full bg-[var(--accent-info-soft)] flex items-center justify-center mx-auto">
              <UserCheck size={32} className="text-[var(--accent-info)]" />
            </div>
            <p className="text-[var(--text-primary)] font-medium">Does the customer already have a loyalty account?</p>
            <div className="flex gap-3">
              <Button variant="secondary" className="flex-1" onClick={() => router.push("/users")}>No (Create)</Button>
              <Button className="flex-1" onClick={() => { setShowAccountCheck(false); setShowBindPoints(true); }}>Yes</Button>
            </div>
          </div>
        </Modal>

        {/* Loyalty Step 3: Bind Points */}
        <Modal isOpen={showBindPoints} onClose={handleSkipPoints} title="Award Points" size="sm">
          <div className="space-y-4">
            <p className="text-sm text-[var(--text-secondary)]">Enter Customer Account ID (Phone/External ID) to award points for this sale.</p>
            <Input label="Account ID" placeholder="Enter ID" value={externalId} onChange={(e) => setExternalId(e.target.value)} icon={<Search size={16} />} />
            <div className="flex gap-3 pt-2">
              <Button variant="secondary" className="flex-1" onClick={handleSkipPoints}>Cancel</Button>
              <Button className="flex-1" onClick={handleEarnPoints} isLoading={pointLoading}>Award Points</Button>
            </div>
          </div>
        </Modal>
      </div>
    </AnimatedPage>
  );
}
