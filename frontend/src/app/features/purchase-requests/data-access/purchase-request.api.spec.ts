import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting
} from '@angular/common/http/testing';

import { PurchaseRequestApi } from './purchase-request.api';

describe('PurchaseRequestApi', () => {
  let api: PurchaseRequestApi;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });

    api = TestBed.inject(PurchaseRequestApi);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    http.verify();
  });

  it('should send a create request', () => {
    const payload = {
      departmentId:
        '22222222-2222-2222-2222-222222222222',
      currency: 'EUR'
    };

    api.create(payload).subscribe(response => {
      expect(response.status).toBe('Draft');
      expect(response.requestNumber).toBe('PR-2026-000001');
    });

    const request = http.expectOne(
      '/api/procurement/purchase-requests'
    );

    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual(payload);

    request.flush({
      id: '11111111-1111-1111-1111-111111111111',
      requestNumber: 'PR-2026-000001',
      status: 'Draft'
    });
  });

    it('should send an add item request', () => {

    const purchaseRequestId =
      '2c3c5d0a-8828-40b7-a052-266e9efbdd25';

    const payload = {
      description: 'Laptop',
      quantity: 2,
      unitPrice: 1500
    };

    api
      .addItem(
        purchaseRequestId,
        payload
      )
      .subscribe(response => {

        expect(
          response.description
        ).toBe('Laptop');

        expect(
          response.lineTotal
        ).toBe(3000);

        expect(
          response.requestTotal
        ).toBe(3000);

      });


    const request =
      http.expectOne(
        `/api/procurement/purchase-requests/${purchaseRequestId}/items`
      );


    expect(
      request.request.method
    ).toBe('POST');


    expect(
      request.request.body
    ).toEqual(payload);


    request.flush({

      itemId:
        'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',

      description:
        'Laptop',

      quantity:
        2,

      unitPrice:
        1500,

      lineTotal:
        3000,

      requestTotal:
        3000,

      currency:
        'EUR'

    });

  });

});