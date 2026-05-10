"use client";

import { useEffect, useState, useCallback, Fragment } from "react";
import { salesApi } from "@/lib/api";
import type { SaleDTO } from "@/lib/types";
import { Card } from "@/components/ui/Card";
import { Input } from "@/components/ui/Input";
import { Button } from "@/components/ui/Button";
import { Badge } from "@/components/ui/Badge";
import { Modal } from "@/components/ui/Modal";
import { SkeletonTable } from "@/components/ui/Skeleton";
import { AnimatedPage } from "@/components/ui/AnimatedPage";
import { toast } from "@/components/ui/Toast";
import { Receipt, ChevronDown, ChevronUp, Search, X } from "lucide-react";

export default function SalesPage() {
  const [sales, setSales] = useState<SaleDTO[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [selectedSale, setSelectedSale] = useState<SaleDTO | null>(null);
  const [expandedId, setExpandedId] = useState<number | null>(null);

  // Voucher Search
  const [voucherSearch, setVoucherSearch] = useState("");
  const [isSearching, setIsSearching] = useState(false);
  const [isSearchMode, setIsSearchMode] = useState(false);

  const loadSales = useCallback(async () => {
    setIsLoading(true);
    try {
      const res = await salesApi.getAll();
      if (res.isSuccess && res.data) setSales(res.data);
    } catch { toast("error", "Failed to load sales"); }
    finally { setIsLoading(false); }
  }, []);

  useEffect(() => { void loadSales(); }, [loadSales]);

  const handleVoucherSearch = async () => {
    if (!voucherSearch.trim()) {
      toast("warning", "Please enter a voucher code");
      return;
    }
    setIsSearching(true);
    setIsSearchMode(true);
    try {
      const res = await salesApi.searchByVoucher(voucherSearch.trim());
      if (res.isSuccess && res.data) {
        setSales(res.data);
        if (res.data.length === 0) {
          toast("info", "No sales found for this voucher code");
        } else {
          toast("success", `Found ${res.data.length} result(s)`);
        }
      } else {
        // Fallback: filter locally if backend doesn't support search
        const allRes = await salesApi.getAll();
        if (allRes.isSuccess && allRes.data) {
          const filtered = allRes.data.filter((s) =>
            s.voucherCode.toLowerCase().includes(voucherSearch.trim().toLowerCase())
          );
          setSales(filtered);
          if (filtered.length === 0) {
            toast("info", "No sales found for this voucher code");
          } else {
            toast("success", `Found ${filtered.length} result(s)`);
          }
        }
      }
    } catch {
      // Fallback: try local filter
      try {
        const allRes = await salesApi.getAll();
        if (allRes.isSuccess && allRes.data) {
          const filtered = allRes.data.filter((s) =>
            s.voucherCode.toLowerCase().includes(voucherSearch.trim().toLowerCase())
          );
          setSales(filtered);
          if (filtered.length === 0) {
            toast("info", "No sales found for this voucher code");
          } else {
            toast("success", `Found ${filtered.length} result(s)`);
          }
        }
      } catch {
        toast("error", "Search failed");
      }
    } finally {
      setIsSearching(false);
    }
  };

  const handleClearSearch = async () => {
    setVoucherSearch("");
    setIsSearchMode(false);
    await loadSales();
  };

  return (
    <AnimatedPage>
      <div className="space-y-6">
        <div>
          <h2 className="text-2xl font-bold text-[var(--text-primary)]">Sales History</h2>
          <p className="text-sm text-[var(--text-secondary)] mt-1">
            {isSearchMode ? `Search results for "${voucherSearch}"` : `${sales.length} transactions`}
          </p>
        </div>

        {/* Voucher Search Bar */}
        <div className="flex flex-col sm:flex-row gap-3">
          <div className="flex-1">
            <Input
              placeholder="Search by Voucher Code..."
              value={voucherSearch}
              onChange={(e) => setVoucherSearch(e.target.value)}
              onKeyDown={(e) => e.key === "Enter" && handleVoucherSearch()}
              icon={<Search size={16} />}
              id="sale-voucher-search"
            />
          </div>
          <div className="flex gap-2">
            <Button onClick={handleVoucherSearch} isLoading={isSearching} icon={<Search size={16} />}>Search</Button>
            {isSearchMode && (
              <Button variant="secondary" onClick={handleClearSearch} icon={<X size={16} />}>Clear</Button>
            )}
          </div>
        </div>

        <Card padding="none">
          {isLoading ? (<div className="p-6"><SkeletonTable rows={8} /></div>) : sales.length === 0 ? (
            <div className="py-16 text-center">
              <Receipt size={48} className="mx-auto mb-3 text-[var(--text-tertiary)] opacity-50" />
              <p className="text-[var(--text-secondary)]">{isSearchMode ? "No sales match this voucher code" : "No sales yet"}</p>
              {isSearchMode && (
                <Button variant="secondary" size="sm" className="mt-3" onClick={handleClearSearch}>Show All Sales</Button>
              )}
            </div>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead>
                  <tr className="border-b border-[var(--border-primary)] bg-[var(--bg-secondary)]">
                    {["#", "Voucher", "Items", "Total", ""].map((h) => (
                      <th key={h} className="text-left py-3 px-4 text-xs font-semibold text-[var(--text-tertiary)] uppercase tracking-wider">{h}</th>
                    ))}
                  </tr>
                </thead>
                <tbody>
                  {sales.map((s) => (
                    <Fragment key={s.id}>
                      <tr className="border-b border-[var(--border-primary)] hover:bg-[var(--bg-hover)] transition-colors cursor-pointer" onClick={() => setExpandedId(expandedId === s.id ? null : s.id)}>
                        <td className="py-3 px-4 text-sm text-[var(--text-secondary)]">#{s.id}</td>
                        <td className="py-3 px-4"><Badge variant="primary"><span className="font-mono">{s.voucherCode}</span></Badge></td>
                        <td className="py-3 px-4 text-sm text-[var(--text-primary)]">{s.saleItems.length} items</td>
                        <td className="py-3 px-4"><span className="text-sm font-bold font-mono text-[var(--text-primary)]">{s.totalPriceFormatted} MMK</span></td>
                        <td className="py-3 px-4 text-right">{expandedId === s.id ? <ChevronUp size={16} className="text-[var(--text-tertiary)]" /> : <ChevronDown size={16} className="text-[var(--text-tertiary)]" />}</td>
                      </tr>
                      {expandedId === s.id && (
                        <tr>
                          <td colSpan={5} className="px-8 py-3 bg-[var(--bg-secondary)]">
                            <div className="space-y-2">
                              {s.saleItems.map((item, idx) => (
                                <div key={idx} className="flex justify-between text-sm">
                                  <span className="text-[var(--text-secondary)]">{item.productName} × {item.quantity}</span>
                                  <span className="font-mono text-[var(--text-primary)]">{(item.price * item.quantity).toLocaleString()} MMK</span>
                                </div>
                              ))}
                            </div>
                          </td>
                        </tr>
                      )}
                    </Fragment>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </Card>

        <Modal isOpen={!!selectedSale} onClose={() => setSelectedSale(null)} title={`Sale ${selectedSale?.voucherCode}`} size="md">
          {selectedSale && (
            <div className="space-y-4">
              <p className="text-2xl font-bold text-[var(--text-primary)]">{selectedSale.totalPriceFormatted} MMK</p>
              {selectedSale.saleItems.map((item, i) => (
                <div key={i} className="flex justify-between py-2 border-b border-[var(--border-primary)] last:border-0">
                  <span className="text-sm text-[var(--text-secondary)]">{item.productName} × {item.quantity}</span>
                  <span className="text-sm font-mono text-[var(--text-primary)]">{(item.price * item.quantity).toLocaleString()} MMK</span>
                </div>
              ))}
            </div>
          )}
        </Modal>
      </div>
    </AnimatedPage>
  );
}
