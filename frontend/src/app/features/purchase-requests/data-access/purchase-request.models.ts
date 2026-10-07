export interface CreatePurchaseRequestRequest {
  departmentId: string;
  currency: string;
}

export interface CreatePurchaseRequestResponse {
  id: string;
  requestNumber: string;
  status: string;
}

export interface AddPurchaseRequestItemRequest {
  description: string;
  quantity: number;
  unitPrice: number;
}

export interface AddPurchaseRequestItemResponse {
  itemId: string;
  description: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
  requestTotal: number;
  currency: string;
}

export interface PurchaseRequestListItem {
  id: string;
  requestNumber: string;
  status: string;
  currency: string;
  totalAmount: number;
  itemCount: number;
  createdAtUtc: string;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface PurchaseRequestListQuery {
  search?: string;
  status?: string;
  sortBy?: string;
  descending?: boolean;
  page?: number;
  pageSize?: number;
}