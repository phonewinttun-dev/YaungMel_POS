// ─── Enums ───────────────────────────────────────────────
export enum UserRole {
  Admin = 0,
  Staff = 1,
  Customer = 2,
}

export type CustomerTier = "None" | "Silver" | "Gold" | "Platinum";
export type RedemptionStatus = "Pending" | "Fulfilled" | "Cancelled";

// ─── Auth ────────────────────────────────────────────────
export interface LoginRequest {
  mobileNum: string;
  password: string;
}

export interface RegisterRequest {
  name: string;
  mobileNum: string;
  password: string;
  role: UserRole;
}

export interface TokenResponse {
  accessToken: string;
  refreshToken: string;
  mobileNum: string;
  role: string;
}

export interface UserResponse {
  isSuccess: boolean;
  message: string;
  userId?: number;
}

export interface UserDTO {
  id: number;
  name: string;
  mobileNum: string;
  role: UserRole;
  createdAt: string;
}

// ─── API Response Wrapper ────────────────────────────────
export interface ApiResponse<T> {
  isSuccess: boolean;
  message: string;
  data: T | null;
}

// ─── Products ────────────────────────────────────────────
export interface ProductDTO {
  id: number;
  name: string;
  description?: string;
  price: number;
  priceFormatted: string;
  stockQuantity: number;
  categoryId: number;
  categoryName?: string;
  isActive: boolean;
  deleteFlag: boolean;
  version?: number;
  imageUrl?: string;
  imageId?: string;
  createdBy?: number;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateProductDTO {
  name: string;
  description?: string;
  price: number;
  stockQuantity: number;
  categoryId: number;
}

export interface UpdateProductDTO {
  name?: string;
  description?: string;
  price?: number;
  stockQuantity?: number;
  categoryId?: number;
  version?: number;
}

// ─── Categories ──────────────────────────────────────────
export interface CategoryDTO {
  id: number;
  name: string;
  description?: string;
  createdAt?: string;
  deleteFlag?: boolean;
}

export interface CreateCategoryDTO {
  name: string;
  description?: string;
}

// ─── Sales ───────────────────────────────────────────────
export interface SaleItemDTO {
  productId: number;
  productName: string;
  quantity: number;
  price: number;
  priceFormatted: string;
}

export interface SaleDTO {
  id: number;
  voucherCode: string;
  totalPrice: number;
  totalPriceFormatted: string;
  saleItems: SaleItemDTO[];
  createdAt?: string;
}

export interface CreateSaleDTO {
  items: { productId: number; quantity: number }[];
}

// ─── Cart ────────────────────────────────────────────────
export interface CartItem {
  product: ProductDTO;
  quantity: number;
}

// ─── Inventory ───────────────────────────────────────────
export interface InventoryAdjustDTO {
  productId: number;
  quantity: number;
}

export interface InventoryPriceDTO {
  productId: number;
  newPrice: number;
}

// ─── Dashboard ───────────────────────────────────────────
export interface SalesOverviewDTO {
  totalRevenue: number;
  totalSales: number;
}

export interface SalesDataPoint {
  label: string;
  totalRevenue: number;
  totalSales: number;
}

export interface SalesPerPeriodDTO {
  period: string;
  data: SalesDataPoint[];
}

export interface TopProductDTO {
  productId: number;
  productName: string;
  totalQuantitySold: number;
  totalRevenue: number;
}

// ─── Points / Loyalty ────────────────────────────────────
export interface AccountItemDTO {
  id: string;
  systemId: string;
  externalUserId: string;
  currentBalance: number;
  lifetimePoints: number;
  tier: string;
  mobile: string;
  email: string;
  updatedAt: string;
}

export interface CreateAccountReqDTO {
  tier: CustomerTier;
  mobile: string;
  email: string;
}

export interface AccountListReqDTO {
  page?: number;
  pageSize?: number;
  systemId?: string;
  tier?: CustomerTier;
  searchTerm?: string;
}

export interface AccountListResponseWrapper {
  items: AccountItemDTO[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface AvailableRewardResDTO {
  id: string;
  name: string;
  description: string;
  pointCost: number;
  stockQuantity: number;
  isActive: boolean;
}

export interface CreateRewardReqDTO {
  name: string;
  description: string;
  pointCost: number;
  stockQuantity: number;
}

export interface UpdateRewardReqDTO {
  name?: string;
  description?: string;
  pointCost?: number;
  stockQuantity?: number;
  isActive?: boolean;
}

export interface EarnPointReqDTO {
  externalUserId: string;
  eventKey: string;
  eventValue: number;
  referenceId: string;
  description: string;
  mobile: string;
  email: string;
}

export interface ClaimRewardReqDTO {
  externalUserId: string;
  rewardId: string;
  notes: string;
}

export interface ClaimRewardResDTO {
  redemptionId?: string;
  status?: string;
  remainingBalance?: number;
}

export interface PointHistoryResDTO {
  pointDelta: number;
  eventKey: string;
  referenceId: string;
  description: string;
  createdAt: string;
}

export interface PendingRedemptionResDTO {
  id: string;
  systemId: string;
  externalUserId: string;
  rewardName: string;
  status: string;
  pointCost: number;
  redeemedAt: string;
}

export interface AccountLookupResponse {
  accountId: string;
  externalUserId: string;
  currentBalance: number;
  lifetimePoints: number;
  tier: string;
}

// ─── Search ──────────────────────────────────────────────
export interface SearchRequestDTO {
  Name?: string;
  CategoryId?: number;
  MinPrice?: number;
  MaxPrice?: number;
  MinStockQuantity?: number;
  MaxStockQuantity?: number;
  SortBy?: string;
  IsDescending?: boolean;
  PageNumber?: number;
  PageSize?: number;
}

export interface SearchCategoryRequestDTO {
  name?: string;
  isDescending?: boolean;
  pageNumber?: number;
  pageSize?: number;
}

// ─── Pagination ──────────────────────────────────────────
export interface PageSettingDTO {
  pageNo: number;
  pageSize: number;
  pageCount: number;
}

// ─── Summaries ───────────────────────────────────────────
export interface SummaryDTO {
  date: string;
  totalSale: number;
  totalAmount: number;
  totalAmountFormatted: string;
  topSaleProductName?: string;
}

export interface SummaryListResponseModel {
  items: SummaryDTO[];
  pageSetting: PageSettingDTO;
}

export interface SaleListResponseModel {
  items: SaleDTO[];
  pageSetting: PageSettingDTO;
}

export interface ProductSearchResponseModel {
  items: ProductDTO[];
  pageSetting: PageSettingDTO;
}

export interface CategoryListResponseModel {
  items: CategoryDTO[];
  pageSetting: PageSettingDTO;
}

export interface SummaryDetailDto {
  summary: SummaryDTO;
  sales: SaleDTO[];
}
