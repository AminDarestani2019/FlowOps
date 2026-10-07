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

    it('should load my purchase requests', () => {

    api
      .getMine({
        search: 'Laptop',
        status: 'Draft',
        sortBy: 'total',
        descending: true,
        page: 1,
        pageSize: 10
      })
      .subscribe(response => {

        expect(
          response.items.length
        ).toBe(1);

        expect(
          response.items[0].requestNumber
        ).toBe(
          'PR-2026-000001'
        );

        expect(
          response.totalCount
        ).toBe(1);

      });


    const request =
      http.expectOne(req =>

        req.url ===
          '/api/procurement/purchase-requests/mine'

        &&

        req.params.get('search') ===
          'Laptop'

        &&

        req.params.get('status') ===
          'Draft'

        &&

        req.params.get('sortBy') ===
          'total'

        &&

        req.params.get('descending') ===
          'true'

        &&

        req.params.get('page') ===
          '1'

        &&

        req.params.get('pageSize') ===
          '10'
      );


    expect(
      request.request.method
    ).toBe('GET');


    request.flush({

      items: [
        {
          id:
            'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',

          requestNumber:
            'PR-2026-000001',

          status:
            'Draft',

          currency:
            'EUR',

          totalAmount:
            3600,

          itemCount:
            2,

          createdAtUtc:
            '2026-10-07T10:00:00Z'
        }
      ],

      page:
        1,

      pageSize:
        10,

      totalCount:
        1,

      totalPages:
        1

    });

  });

    it('should load a purchase request by id', () => {

    const id =
      'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa';

    api
      .getById(id)
      .subscribe(response => {

        expect(
          response.id
        ).toBe(id);

        expect(
          response.requestNumber
        ).toBe(
          'PR-2026-000001'
        );

        expect(
          response.totalAmount
        ).toBe(3600);

        expect(
          response.items.length
        ).toBe(2);

      });


    const request =
      http.expectOne(
        `/api/procurement/purchase-requests/${id}`
      );


    expect(
      request.request.method
    ).toBe('GET');


    request.flush({

      id,

      requestNumber:
        'PR-2026-000001',

      requesterId:
        '11111111-1111-1111-1111-111111111111',

      departmentId:
        '22222222-2222-2222-2222-222222222222',

      currency:
        'EUR',

      status:
        'Draft',

      createdAtUtc:
        '2026-10-07T10:00:00Z',

      submittedAtUtc:
        null,

      totalAmount:
        3600,

      items: [

        {
          id:
            'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',

          description:
            'Development Laptop',

          quantity:
            2,

          unitPrice:
            1500,

          lineTotal:
            3000
        },

        {
          id:
            'cccccccc-cccc-cccc-cccc-cccccccccccc',

          description:
            'Docking Station',

          quantity:
            3,

          unitPrice:
            200,

          lineTotal:
            600
        }

      ]

    });

  });

    it('should submit a purchase request', () => {

    const id =
      'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa';

    api
      .submit(id)
      .subscribe(response => {

        expect(
          response.status
        ).toBe('Submitted');

        expect(
          response.id
        ).toBe(id);

        expect(
          response.submittedAtUtc
        ).toBe(
          '2026-10-07T12:00:00Z'
        );

      });


    const request =
      http.expectOne(
        `/api/procurement/purchase-requests/${id}/submit`
      );


    expect(
      request.request.method
    ).toBe('POST');


    request.flush({

      id,

      requestNumber:
        'PR-2026-000001',

      status:
        'Submitted',

      submittedAtUtc:
        '2026-10-07T12:00:00Z'

    });

  });

});