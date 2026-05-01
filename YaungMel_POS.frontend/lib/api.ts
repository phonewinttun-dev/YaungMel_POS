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
    api.get<ApiResponse<ProductDTO[]>>("/api/products").then(unwrap),

  getById: (id: number) =>
    api.get<ApiResponse<ProductDTO>>(`/api/products/${id}`).then(unwrap),

  getAvailable: () =>
    api.get<ApiResponse<ProductDTO[]>>("/api/products/availableProducts").then(unwrap),

  create: (data: CreateProductDTO) =>
    api.post<ApiResponse<ProductDTO>>("/api/products", data).then(unwrap),

  bulkCreate: (data: CreateProductDTO[]) =>
    api.post<ApiResponse<ProductDTO[]>>("/api/products/bulk", data).then(unwrap),

  update: (id: number, data: UpdateProductDTO) =>
    api.patch<ApiResponse<ProductDTO>>(`/api/products/${id}`, data).then(unwrap),

  delete: (id: number) =>
    api.delete<ApiResponse<object>>(`/api/products/${id}`).then(unwrap),

  search: (term: string) =>
    api.get<ApiResponse<ProductDTO[]>>(`/api/products/search?term=${encodeURIComponent(term)}`).then(unwrap),
};

// ─── Categories API ───────────────────────────────────────
export const categoriesApi = {
  getAll: () =>
    api.get<ApiResponse<CategoryDTO[]>>("/api/categories").then(unwrap),

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
    api.get<ApiResponse<SaleDTO[]>>("/api/sales").then(unwrap),

  getById: (id: number) =>
    api.get<ApiResponse<SaleDTO>>(`/api/sales/${id}`).then(unwrap),

  create: (data: CreateSaleDTO) =>
    api.post<ApiResponse<SaleDTO>>("/api/sales", data).then(unwrap),

  searchByVoucher: (voucherCode: string) =>
    api.get<ApiResponse<SaleDTO[]>>(`/api/sales/search?voucherCode=${encodeURIComponent(voucherCode)}`).then(unwrap),
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
    api.get<ApiResponse<ProductDTO[]>>("/api/search", { params }).then(unwrap),
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
