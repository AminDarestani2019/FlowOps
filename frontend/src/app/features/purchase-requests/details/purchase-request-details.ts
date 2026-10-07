import {
  Component,
  computed,
  DestroyRef,
  inject,
  signal
} from '@angular/core';

import {
  DatePipe,
  DecimalPipe
} from '@angular/common';

import {
  ActivatedRoute,
  RouterLink
} from '@angular/router';

import {
  HttpErrorResponse
} from '@angular/common/http';

import {
  takeUntilDestroyed
} from '@angular/core/rxjs-interop';

import {
  finalize
} from 'rxjs';

import {
  PurchaseRequestApi
} from '../data-access/purchase-request.api';

import {
  PurchaseRequestDetails
} from '../data-access/purchase-request.models';


@Component({
  selector: 'app-purchase-request-details',
  standalone: true,
  imports: [
    RouterLink,
    DatePipe,
    DecimalPipe
  ],
  templateUrl: './purchase-request-details.html',
  styleUrl: './purchase-request-details.scss'
})
export class PurchaseRequestDetailsComponent {

  private readonly route =
    inject(ActivatedRoute);

  private readonly api =
    inject(PurchaseRequestApi);

  private readonly destroyRef =
    inject(DestroyRef);


  readonly loading =
    signal(true);

  readonly errorMessage =
    signal<string | null>(null);

  readonly purchaseRequest =
    signal<PurchaseRequestDetails | null>(null);

  readonly submitting =
    signal(false);

  readonly submitError =
    signal<string | null>(null);

  readonly submitSuccess =
    signal<string | null>(null);

  readonly confirmingSubmit =
    signal(false);

  readonly canSubmit =
    computed(() => {

      const request =
        this.purchaseRequest();

      if (!request) {
        return false;
      }

      return (
        request.status === 'Draft' &&
        request.items.length > 0
      );
    });

  constructor() {

    const purchaseRequestId =
      this.route.snapshot.paramMap.get('id');

    if (!purchaseRequestId) {

      this.loading.set(false);

      this.errorMessage.set(
        'Purchase request ID is missing.'
      );

      return;
    }

    this.load(
      purchaseRequestId
    );
  }

  requestSubmit(): void {

    if (!this.canSubmit()) {
      return;
    }

    this.submitError.set(null);
    this.submitSuccess.set(null);

    this.confirmingSubmit.set(true);
  }

  cancelSubmit(): void {

    if (this.submitting()) {
      return;
    }

    this.confirmingSubmit.set(false);
  }

  private load(
    purchaseRequestId: string
  ): void {

    this.loading.set(true);
    this.errorMessage.set(null);

    this.api
      .getById(
        purchaseRequestId
      )
      .pipe(

        takeUntilDestroyed(
          this.destroyRef
        ),

        finalize(() =>
          this.loading.set(false)
        )
      )
      .subscribe({

        next: response => {

          this.purchaseRequest.set(
            response
          );
        },

        error: (error: unknown) => {

          this.purchaseRequest.set(null);

          this.errorMessage.set(
            this.getErrorMessage(
              error
            )
          );
        }

      });
  }

  private getErrorMessage(
    error: unknown
  ): string {

    if (
      !(error instanceof HttpErrorResponse)
    ) {
      return 'An unexpected error occurred.';
    }

    if (error.status === 0) {
      return 'Cannot connect to the FlowOps API.';
    }

    if (error.status === 404) {
      return 'Purchase request was not found.';
    }

    const problem =
      error.error as {
        title?: string;
        detail?: string;
      } | null;

    return (
      problem?.detail
      ?? problem?.title
      ?? `Request failed with HTTP ${error.status}.`
    );
  }

  confirmSubmit(): void {

    const request =
      this.purchaseRequest();

    if (
      !request ||
      !this.canSubmit() ||
      this.submitting()
    ) {
      return;
    }

    this.submitting.set(true);
    this.submitError.set(null);
    this.submitSuccess.set(null);

    this.api
      .submit(request.id)
      .pipe(

        takeUntilDestroyed(
          this.destroyRef
        ),

        finalize(() =>
          this.submitting.set(false)
        )
      )
      .subscribe({

        next: response => {

          const current =
            this.purchaseRequest();

          if (!current) {
            return;
          }

          this.purchaseRequest.set({
            ...current,
            status:
              response.status,

            submittedAtUtc:
              response.submittedAtUtc
          });

          this.confirmingSubmit.set(false);

          this.submitSuccess.set(
            'Purchase request submitted successfully.'
          );
        },

        error: (error: unknown) => {

          this.submitError.set(
            this.getSubmitErrorMessage(
              error
            )
          );
        }

      });
  }

  private getSubmitErrorMessage(
    error: unknown
  ): string {

    if (
      !(error instanceof HttpErrorResponse)
    ) {
      return 'An unexpected error occurred while submitting the purchase request.';
    }

    if (error.status === 0) {
      return 'Cannot connect to the FlowOps API.';
    }

    if (error.status === 404) {
      return 'Purchase request was not found.';
    }

    if (error.status === 409) {
      return 'This purchase request can no longer be submitted.';
    }

    const problem =
      error.error as {
        title?: string;
        detail?: string;
      } | null;

    return (
      problem?.detail
      ?? problem?.title
      ?? `Submit failed with HTTP ${error.status}.`
    );
  }

}