import { inject, Injectable } from '@angular/core';
import { HttpClient,HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

import {
  AddPurchaseRequestItemRequest,
  AddPurchaseRequestItemResponse,
  CreatePurchaseRequestRequest,
  CreatePurchaseRequestResponse,
  PagedResult,
  PurchaseRequestListItem,
  PurchaseRequestListQuery
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

    getMine(
    query: PurchaseRequestListQuery
  ): Observable<PagedResult<PurchaseRequestListItem>> {

    let params =
      new HttpParams();

    if (query.search?.trim()) {
      params =
        params.set(
          'search',
          query.search.trim()
        );
    }

    if (query.status?.trim()) {
      params =
        params.set(
          'status',
          query.status
        );
    }

    if (query.sortBy) {
      params =
        params.set(
          'sortBy',
          query.sortBy
        );
    }

    if (query.descending !== undefined) {
      params =
        params.set(
          'descending',
          query.descending
        );
    }

    if (query.page !== undefined) {
      params =
        params.set(
          'page',
          query.page
        );
    }

    if (query.pageSize !== undefined) {
      params =
        params.set(
          'pageSize',
          query.pageSize
        );
    }

    return this.http.get<
      PagedResult<PurchaseRequestListItem>
    >(
      `${this.baseUrl}/mine`,
      {
        params
      }
    );
  }

}