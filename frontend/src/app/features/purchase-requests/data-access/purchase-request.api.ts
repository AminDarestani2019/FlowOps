import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import {
  AddPurchaseRequestItemRequest,
  AddPurchaseRequestItemResponse,
  CreatePurchaseRequestRequest,
  CreatePurchaseRequestResponse,
} from './purchase-request.models';

@Injectable({
  providedIn: 'root'
})
export class PurchaseRequestApi {
  private readonly http = inject(HttpClient);

  private readonly baseUrl =
    '/api/procurement/purchase-requests';

  create(
    request: CreatePurchaseRequestRequest
  ): Observable<CreatePurchaseRequestResponse> {
    return this.http.post<CreatePurchaseRequestResponse>(
      this.baseUrl,
      request
    );
  }


  addItem(
    purchaseRequestId: string,
    request: AddPurchaseRequestItemRequest
  ): Observable<AddPurchaseRequestItemResponse> {

    return this.http.post<AddPurchaseRequestItemResponse>(
      `${this.baseUrl}/${purchaseRequestId}/items`,
      request
    );
  }

}