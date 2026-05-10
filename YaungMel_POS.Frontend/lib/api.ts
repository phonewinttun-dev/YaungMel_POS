import axios from "axios";
import type {
  ApiResponse,
  LoginRequest,
  RegisterRequest,
  TokenResponse,
  ProductDTO,
  CreateProductDTO,
  UpdateProductDTO,
  CategoryDTO,
  CreateCategoryDTO,
  SaleDTO,
  CreateSaleDTO,
  InventoryAdjustDTO,
  InventoryPriceDTO,
  SalesOverviewDTO,
  SalesPerPeriodDTO,
  TopProductDTO,
  SearchRequestDTO,
  CreateAccountReqDTO,
  AccountListReqDTO,
  AccountListResponseWrapper,
  AvailableRewardResDTO,
  EarnPointReqDTO,
  ClaimRewardReqDTO,
  ClaimRewardResDTO,
  PointHistoryResDTO,
  PendingRedemptionResDTO,
  AccountLookupResponse,
  RedemptionStatus,
  UserResponse,
  CreateRewardReqDTO,
  UpdateRewardReqDTO,
  SearchCategoryRequestDTO,
  SummaryDTO,
  SummaryListResponseModel,
  SummaryDetailDto,
  SaleListResponseModel,
  ProductSearchResponseModel,
} from "./types";

const API_BASE = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5000";

const api = axios.create({
  baseURL: API_BASE,
  headers: { "Content-Type": "application/json" },
});

// ─── Request interceptor: attach token ────────────────────
api.interceptors.request.use((config) => {
  if (typeof window !== "undefined") {
    const token = localStorage.getItem("accessToken");
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
  }
  return config;
});

// ─── Response interceptor: handle 401 ─────────────────────
api.interceptors.response.use(
  (res) => res,
  (error) => {
    if (error.response?.status === 401 && typeof window !== "undefined") {
      localStorage.removeItem("accessToken");
      localStorage.removeItem("user");
      window.location.href = "/login";
    }
    return Promise.reject(error);
  }
);

// Helper to safely extract ApiResponse from axios
function unwrap<T>(res: { data: ApiResponse<T> }): ApiResponse<T> {
  return res.data;
}

function normalizeCategoryListData(data: unknown): CategoryDTO[] {
  if (Array.isArray(data)) return data as CategoryDTO[];
  if (data && typeof data === "object" && "items" in data) {
    const items = (data as { items?: unknown }).items;
    if (Array.isArray(items)) return items as CategoryDTO[];
  }
  return [];
}

function normalizeProductListData(data: unknown): ProductDTO[] {
  if (Array.isArray(data)) return data as ProductDTO[];
  if (data && typeof data === "object" && "items" in data) {
    const items = (data as { items?: unknown }).items;
    if (Array.isArray(items)) return items as ProductDTO[];
  }
  return [];
}

function normalizeSalesListData(data: unknown): SaleDTO[] {
  if (Array.isArray(data)) return data as SaleDTO[];
  if (data && typeof data === "object" && "items" in data) {
    const items = (data as { items?: unknown }).items;
    if (Array.isArray(items)) return items as SaleDTO[];
  }
  return [];
}

// ─── Auth API ─────────────────────────────────────────────
export const authApi = {
  login: (data: LoginRequest) =>
    api.post<ApiResponse<TokenResponse>>("/api/auth/login", data).then(unwrap),

  register: (data: RegisterRequest) =>
    api.post<ApiResponse<UserResponse>>("/api/auth/register", data).then(unwrap),

  deleteUser: (id: number) =>
    api.delete<ApiResponse<object>>(`/api/auth/users/${id}`).then(unwrap),
};

// ─── Products API ─────────────────────────────────────────
export const productsApi = {
  getAll: () =>
    api.get<ApiResponse<unknown>>("/api/products/paged?pageNo=1&pageSize=500").then((res) => {
      const raw = unwrap(res);
      return {
        ...raw,
        data: normalizeProductListData(raw.data),
      } as ApiResponse<ProductDTO[]>;
    }),

  getById: (id: number) =>
    api.get<ApiResponse<ProductDTO>>(`/api/products/${id}`).then(unwrap),

  getAvailable: () =>
    api.get<ApiResponse<unknown>>("/api/products/paged?pageNo=1&pageSize=500").then((res) => {
      const raw = unwrap(res);
      return {
        ...raw,
        data: normalizeProductListData(raw.data),
      } as ApiResponse<ProductDTO[]>;
    }),

  create: (data: CreateProductDTO | FormData) => {
    if (data instanceof FormData) {
      return api.post<ApiResponse<ProductDTO>>("/api/products", data).then(unwrap);
    }
    const payload = new FormData();
    payload.append("Name", data.name);
    if (data.description) payload.append("Description", data.description);
    payload.append("Price", String(data.price));
    payload.append("StockQuantity", String(data.stockQuantity));
    payload.append("CategoryId", String(data.categoryId));
    return api.post<ApiResponse<ProductDTO>>("/api/products", payload).then(unwrap);
  },

  createWithPhoto: (data: FormData) =>
    api.post<ApiResponse<ProductDTO>>("/api/products", data).then(unwrap),

  bulkCreate: (data: CreateProductDTO[]) =>
    api.post<ApiResponse<ProductDTO[]>>("/api/products/bulk", data).then(unwrap),

  update: (id: number, data: UpdateProductDTO) => {
    const payload = new FormData();
    if (data.name !== undefined) payload.append("Name", data.name);
    if (data.description !== undefined) payload.append("Description", data.description);
    if (data.price !== undefined) payload.append("Price", String(data.price));
    if (data.stockQuantity !== undefined) payload.append("StockQuantity", String(data.stockQuantity));
    if (data.categoryId !== undefined) payload.append("CategoryId", String(data.categoryId));
    if (data.version !== undefined) payload.append("Version", String(data.version));

    return api
      .patch<ApiResponse<ProductDTO>>(`/api/products/${id}`, payload)
      .then(unwrap);
  },

  updateWithPhoto: (id: number, data: FormData) =>
    api
      .patch<ApiResponse<ProductDTO>>(`/api/products/${id}`, data)
      .then(unwrap),

  delete: (id: number) =>
    api.delete<ApiResponse<object>>(`/api/products/${id}`).then(unwrap),

  search: (term: string) =>
    api.get<ApiResponse<ProductDTO[]>>(`/api/products/search?term=${encodeURIComponent(term)}`).then(unwrap),
};

// ─── Categories API ───────────────────────────────────────
export const categoriesApi = {
  getAll: () =>
    api.get<ApiResponse<unknown>>("/api/categories?pageSize=100").then((res) => {
      const raw = unwrap(res);
      return {
        ...raw,
        data: normalizeCategoryListData(raw.data),
      } as ApiResponse<CategoryDTO[]>;
    }),
  
  getPaged: (pageNo: number, pageSize: number) =>
    api.get<ApiResponse<SummaryListResponseModel>>(`/api/categories/paged?pageNo=${pageNo}&pageSize=${pageSize}`).then(unwrap),

  getById: (id: number) =>
    api.get<ApiResponse<CategoryDTO>>(`/api/categories/${id}`).then(unwrap),

  create: (data: CreateCategoryDTO) =>
    api.post<ApiResponse<CategoryDTO>>("/api/categories", data).then(unwrap),

  update: (id: number, data: Partial<CreateCategoryDTO>) =>
    api.patch<ApiResponse<CategoryDTO>>(`/api/categories/${id}`, data).then(unwrap),

  delete: (id: number) =>
    api.delete<ApiResponse<object>>(`/api/categories/${id}`).then(unwrap),

  search: (term: string) =>
    api.get<ApiResponse<CategoryDTO[]>>(`/api/categories/search?term=${encodeURIComponent(term)}`).then(unwrap),
};

// ─── Sales API ────────────────────────────────────────────
export const salesApi = {
  getAll: () =>
    api.get<ApiResponse<unknown>>("/api/sales/paged?pageNo=1&pageSize=500").then((res) => {
      const raw = unwrap(res);
      return {
        ...raw,
        data: normalizeSalesListData(raw.data),
      } as ApiResponse<SaleDTO[]>;
    }),

  getPaged: (pageNo: number, pageSize: number) =>
    api.get<ApiResponse<SaleListResponseModel>>(`/api/sales/paged?pageNo=${pageNo}&pageSize=${pageSize}`).then(unwrap),

  getById: (id: number) =>
    api.get<ApiResponse<SaleDTO>>(`/api/sales/${id}`).then(unwrap),

  create: (data: CreateSaleDTO) =>
    api.post<ApiResponse<SaleDTO>>("/api/sales", data).then(unwrap),

  searchByVoucher: (voucherCode: string) =>
    api.get<ApiResponse<SaleDTO>>(`/api/sales/${encodeURIComponent(voucherCode)}`).then((res) => {
      const raw = unwrap(res);
      return {
        ...raw,
        data: raw.data ? [raw.data] : [],
      } as ApiResponse<SaleDTO[]>;
    }),
};

// ─── Inventory API ────────────────────────────────────────
export const inventoryApi = {
  increaseStock: (data: InventoryAdjustDTO) =>
    api.patch<ApiResponse<object>>("/api/inventory/increase-stock", data).then(unwrap),

  reduceStock: (data: InventoryAdjustDTO) =>
    api.patch<ApiResponse<object>>("/api/inventory/reduce-stock", data).then(unwrap),

  getLowStock: (threshold: number) =>
    api.get<ApiResponse<ProductDTO[]>>(`/api/inventory/low-stock?lowStock=${threshold}`).then(unwrap),

  updatePrice: (id: number, data: InventoryPriceDTO) =>
    api.patch<ApiResponse<object>>(`/api/inventory/${id}`, data).then(unwrap),
};

// ─── Dashboard API ────────────────────────────────────────
export const dashboardApi = {
  getOverview: (startDate: string, endDate: string) =>
    api.get<ApiResponse<SalesOverviewDTO>>(`/api/dashboard/overview?startDate=${startDate}&endDate=${endDate}`).then(unwrap),

  getSalesPerPeriod: (period: string) =>
    api.get<ApiResponse<SalesPerPeriodDTO>>(`/api/dashboard/sales-per-period?period=${period}`).then(unwrap),

  getReport: (range: string) =>
    api.get<ApiResponse<object>>(`/api/dashboard/report?range=${range}`).then(unwrap),

  getTopProducts: (top: number) =>
    api.get<ApiResponse<TopProductDTO[]>>(`/api/dashboard/top-products?top=${top}`).then(unwrap),
};

// ─── Search API ───────────────────────────────────────────
export const searchApi = {
  search: (params: SearchRequestDTO) =>
    api.get<ApiResponse<ProductSearchResponseModel>>("/api/search", { params }).then(unwrap),

  searchCategories: (params: SearchCategoryRequestDTO) =>
    api.get<ApiResponse<CategoryDTO[]>>("/api/search/categories", { params }).then(unwrap),
};

// ─── Reports API ──────────────────────────────────────────
export const reportsApi = {
  generateDaily: (date: string) =>
    api.get(`/api/reports?date=${date}`, { responseType: "blob" }).then((res) => res.data as Blob),

  generateRange: (startDate: string, endDate: string) =>
    api.get(`/api/reports/range?startDate=${startDate}&endDate=${endDate}`, { responseType: "blob" }).then((res) => res.data as Blob),
};

// ─── Summaries API ────────────────────────────────────────
export const summariesApi = {
  create: () =>
    api.post<ApiResponse<SummaryDTO>>("/api/summaries/create").then(unwrap),

  getAll: () =>
    api.get<ApiResponse<SummaryDTO[]>>("/api/summaries").then(unwrap),

  getPaged: (pageNo: number, pageSize: number) =>
    api.get<ApiResponse<SummaryListResponseModel>>(`/api/summaries/paged?pageNo=${pageNo}&pageSize=${pageSize}`).then(unwrap),

  getByDate: (date: string) =>
    api.get<ApiResponse<SummaryDetailDto>>(`/api/summaries/by-date?date=${date}`).then(unwrap),

  getByDateRange: (startDate: string, endDate: string) =>
    api.get<ApiResponse<SummaryDTO[]>>(`/api/summaries/by-date-range?startDate=${startDate}&endDate=${endDate}`).then(unwrap),
};

// ─── Points / Loyalty API ─────────────────────────────────
export const pointsApi = {
  createAccount: (data: CreateAccountReqDTO) =>
    api.post<ApiResponse<object>>("/api/points/accounts", data).then(unwrap),

  getAccounts: (params: AccountListReqDTO) =>
    api.get<ApiResponse<AccountListResponseWrapper>>("/api/points/accounts", { params }).then(unwrap),

  lookupAccount: (userId: string) =>
    api.get<ApiResponse<AccountLookupResponse>>(`/api/points/accounts/lookup/${userId}`).then(unwrap),

  getBalance: (params: { systemId?: string; externalUserId: string }) =>
    api.get<ApiResponse<object>>("/api/points/balance-lookup", { params }).then(unwrap),

  earnPoints: (data: EarnPointReqDTO) =>
    api.post<ApiResponse<object>>("/api/points/earn", data).then(unwrap),

  getAvailableRewards: () =>
    api.get<ApiResponse<AvailableRewardResDTO[]>>("/api/points/rewards/available").then(unwrap),

  claimReward: (data: ClaimRewardReqDTO) =>
    api.post<ApiResponse<ClaimRewardResDTO>>("/api/points/redemption/claim", data).then(unwrap),

  getPointHistory: (accountId: string) =>
    api.get<ApiResponse<PointHistoryResDTO[]>>(`/api/points/accounts/${accountId}/history`).then(unwrap),

  getPendingRedemptions: () =>
    api.get<ApiResponse<PendingRedemptionResDTO[]>>("/api/points/admin/redemptions/pending").then(unwrap),

  updateRedemptionStatus: (id: string, status: RedemptionStatus) =>
    api.put<ApiResponse<object>>(`/api/points/admin/redemptions/${id}/status`, JSON.stringify(status)).then(unwrap),

  createReward: (data: CreateRewardReqDTO) =>
    api.post<ApiResponse<AvailableRewardResDTO>>("/api/points/rewards", data).then(unwrap),

  updateReward: (id: string, data: UpdateRewardReqDTO) =>
    api.put<ApiResponse<AvailableRewardResDTO>>(`/api/points/rewards/${id}`, data).then(unwrap),

  deleteReward: (id: string) =>
    api.delete<ApiResponse<object>>(`/api/points/rewards/${id}`).then(unwrap),
};
