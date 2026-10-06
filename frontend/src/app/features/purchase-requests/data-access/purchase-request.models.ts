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