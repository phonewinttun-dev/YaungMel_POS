"use client";

import { useEffect, useState } from "react";
import { Search as SearchIcon, Filter, RotateCcw, Boxes } from "lucide-react";
import { searchApi, categoriesApi } from "@/lib/api";
import type { CategoryDTO, ProductDTO, SearchRequestDTO } from "@/lib/types";
import { Card, CardHeader } from "@/components/ui/Card";
import { Input } from "@/components/ui/Input";
import { Button } from "@/components/ui/Button";
import { Badge } from "@/components/ui/Badge";
import { SkeletonTable } from "@/components/ui/Skeleton";
import { AnimatedPage } from "@/components/ui/AnimatedPage";
import { toast } from "@/components/ui/Toast";

const initialFilters: SearchRequestDTO = {
  name: "", categoryId: undefined, minPrice: undefined, maxPrice: undefined,
  minStockQuantity: undefined, maxStockQuantity: undefined,
  sortBy: "Name", isDescending: false, pageNumber: 1, pageSize: 20,
};

export default function SearchPage() {
  const [categories, setCategories] = useState<CategoryDTO[]>([]);
  const [results, setResults] = useState<ProductDTO[]>([]);
  const [filters, setFilters] = useState<SearchRequestDTO>(initialFilters);
  const [isLoading, setIsLoading] = useState(true);
  const [isSearching, setIsSearching] = useState(false);

  useEffect(() => {
    const timer = window.setTimeout(async () => {
      setIsLoading(true);
      try {
        const [categoryRes, searchRes] = await Promise.all([categoriesApi.getAll(), searchApi.search(initialFilters)]);
        if (categoryRes.isSuccess && categoryRes.data) setCategories(categoryRes.data);
        if (searchRes.isSuccess && searchRes.data) setResults(searchRes.data);
        else toast("error", searchRes.message || "Search failed");
      } catch { toast("error", "Failed to load search tools"); }
      finally { setIsLoading(false); }
    }, 0);
    return () => window.clearTimeout(timer);
  }, []);

  const handleSearch = async () => {
    setIsSearching(true);
    try {
      const res = await searchApi.search(filters);
      if (res.isSuccess && res.data) setResults(res.data);
      else toast("error", res.message || "Search failed");
    } catch { toast("error", "Search request failed"); }
    finally { setIsSearching(false); }
  };

  const handleReset = async () => {
    setFilters(initialFilters);
    setIsSearching(true);
    try {
      const res = await searchApi.search(initialFilters);
      if (res.isSuccess && res.data) setResults(res.data);
    } catch { toast("error", "Failed to reset search"); }
    finally { setIsSearching(false); }
  };

  const getCategoryName = (id: number) => categories.find((c) => c.id === id)?.name || "Unknown";

  return (
    <AnimatedPage>
      <div className="space-y-6">
        <div>
          <h2 className="text-2xl font-bold text-[var(--text-primary)]">Advanced Search</h2>
          <p className="text-sm text-[var(--text-secondary)] mt-1">Filter products by name, category, price, and stock.</p>
        </div>

        <Card padding="lg">
          <CardHeader title="Search Filters" subtitle="Build a product query and fetch matching items." action={<Filter size={18} className="text-[var(--text-tertiary)]" />} />
          <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-4 gap-4">
            <Input label="Product Name" placeholder="Search by name" value={filters.name || ""} onChange={(e) => setFilters((prev) => ({ ...prev, name: e.target.value }))} icon={<SearchIcon size={16} />} />
            <div>
              <label className="block text-sm font-medium text-[var(--text-secondary)] mb-1.5">Category</label>
              <select value={filters.categoryId ?? ""} onChange={(e) => setFilters((prev) => ({ ...prev, categoryId: e.target.value ? Number(e.target.value) : undefined }))} className="w-full px-4 py-2.5 text-sm rounded-xl bg-[var(--bg-input)] border border-[var(--border-primary)] text-[var(--text-primary)] focus:outline-none focus:ring-2 focus:ring-[var(--accent-primary)]">
                <option value="">All Categories</option>
                {categories.map((c) => (<option key={c.id} value={c.id}>{c.name}</option>))}
              </select>
            </div>
            <Input label="Min Price" type="number" placeholder="0" value={filters.minPrice ?? ""} onChange={(e) => setFilters((prev) => ({ ...prev, minPrice: e.target.value ? Number(e.target.value) : undefined }))} />
            <Input label="Max Price" type="number" placeholder="10000" value={filters.maxPrice ?? ""} onChange={(e) => setFilters((prev) => ({ ...prev, maxPrice: e.target.value ? Number(e.target.value) : undefined }))} />
            <Input label="Min Stock" type="number" placeholder="0" value={filters.minStockQuantity ?? ""} onChange={(e) => setFilters((prev) => ({ ...prev, minStockQuantity: e.target.value ? Number(e.target.value) : undefined }))} />
            <Input label="Max Stock" type="number" placeholder="100" value={filters.maxStockQuantity ?? ""} onChange={(e) => setFilters((prev) => ({ ...prev, maxStockQuantity: e.target.value ? Number(e.target.value) : undefined }))} />
            <div>
              <label className="block text-sm font-medium text-[var(--text-secondary)] mb-1.5">Sort By</label>
              <select value={filters.sortBy ?? "Name"} onChange={(e) => setFilters((prev) => ({ ...prev, sortBy: e.target.value }))} className="w-full px-4 py-2.5 text-sm rounded-xl bg-[var(--bg-input)] border border-[var(--border-primary)] text-[var(--text-primary)] focus:outline-none focus:ring-2 focus:ring-[var(--accent-primary)]">
                <option value="Name">Name</option><option value="Price">Price</option><option value="CreatedAt">Created Date</option>
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium text-[var(--text-secondary)] mb-1.5">Order</label>
              <select value={filters.isDescending ? "desc" : "asc"} onChange={(e) => setFilters((prev) => ({ ...prev, isDescending: e.target.value === "desc" }))} className="w-full px-4 py-2.5 text-sm rounded-xl bg-[var(--bg-input)] border border-[var(--border-primary)] text-[var(--text-primary)] focus:outline-none focus:ring-2 focus:ring-[var(--accent-primary)]">
                <option value="asc">Ascending</option><option value="desc">Descending</option>
              </select>
            </div>
          </div>
          <div className="flex flex-wrap gap-3 mt-6">
            <Button onClick={handleSearch} isLoading={isSearching} icon={<SearchIcon size={16} />}>Run Search</Button>
            <Button variant="secondary" onClick={handleReset} icon={<RotateCcw size={16} />}>Reset</Button>
          </div>
        </Card>

        <Card padding="none">
          {isLoading ? (<div className="p-6"><SkeletonTable rows={6} /></div>) : results.length === 0 ? (
            <div className="py-16 text-center">
              <Boxes size={48} className="mx-auto mb-3 text-[var(--text-tertiary)] opacity-50" />
              <p className="text-[var(--text-secondary)]">No products matched your filters.</p>
            </div>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead>
                  <tr className="border-b border-[var(--border-primary)] bg-[var(--bg-secondary)]">
                    {["Product", "Category", "Price", "Stock", "Status"].map((h) => (
                      <th key={h} className="text-left py-3 px-4 text-xs font-semibold text-[var(--text-tertiary)] uppercase tracking-wider">{h}</th>
                    ))}
                  </tr>
                </thead>
                <tbody>
                  {results.map((product) => (
                    <tr key={product.id} className="border-b border-[var(--border-primary)] last:border-0 hover:bg-[var(--bg-hover)] transition-colors">
                      <td className="py-3 px-4">
                        <p className="text-sm font-medium text-[var(--text-primary)]">{product.name}</p>
                        {product.description && <p className="text-xs text-[var(--text-tertiary)] mt-0.5">{product.description}</p>}
                      </td>
                      <td className="py-3 px-4"><Badge variant="info">{getCategoryName(product.categoryId)}</Badge></td>
                      <td className="py-3 px-4 text-sm font-mono text-[var(--text-primary)]">${product.price.toFixed(2)}</td>
                      <td className="py-3 px-4"><Badge variant={product.stockQuantity <= 5 ? "warning" : "success"}>{product.stockQuantity}</Badge></td>
                      <td className="py-3 px-4"><Badge variant={product.isActive ? "success" : "danger"}>{product.isActive ? "Active" : "Inactive"}</Badge></td>
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
