import {
  Component,
  computed,
  DestroyRef,
  inject,
  signal
} from '@angular/core';

import {
  FormControl,
  FormGroup,
  ReactiveFormsModule
} from '@angular/forms';

import {
  DatePipe,
  DecimalPipe
} from '@angular/common';

import {
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
  PagedResult,
  PurchaseRequestListItem
} from '../data-access/purchase-request.models';


@Component({
  selector: 'app-purchase-request-list',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink,
    DatePipe,
    DecimalPipe
  ],
  templateUrl: './purchase-request-list.html',
  styleUrl: './purchase-request-list.scss'
})
export class PurchaseRequestListComponent {

  private readonly api =
    inject(PurchaseRequestApi);

  private readonly destroyRef =
    inject(DestroyRef);


  readonly loading =
    signal(false);

  readonly errorMessage =
    signal<string | null>(null);

  readonly result =
    signal<PagedResult<PurchaseRequestListItem> | null>(
      null
    );

  readonly page =
    signal(1);


  readonly filterForm =
    new FormGroup({

      search:
        new FormControl(
          '',
          {
            nonNullable: true
          }
        ),

      status:
        new FormControl(
          '',
          {
            nonNullable: true
          }
        ),

      sortBy:
        new FormControl(
          'createdAt',
          {
            nonNullable: true
          }
        ),

      descending:
        new FormControl(
          true,
          {
            nonNullable: true
          }
        ),

      pageSize:
        new FormControl(
          10,
          {
            nonNullable: true
          }
        )

    });


  readonly hasPreviousPage =
    computed(() =>
      this.page() > 1
    );


  readonly hasNextPage =
    computed(() => {

      const result =
        this.result();

      if (!result) {
        return false;
      }

      return (
        this.page() <
        result.totalPages
      );
    });


  readonly showingFrom =
    computed(() => {

      const result =
        this.result();

      if (
        !result ||
        result.totalCount === 0
      ) {
        return 0;
      }

      return (
        (result.page - 1) *
        result.pageSize +
        1
      );
    });


  readonly showingTo =
    computed(() => {

      const result =
        this.result();

      if (!result) {
        return 0;
      }

      return Math.min(
        result.page *
        result.pageSize,
        result.totalCount
      );
    });


  constructor() {
    this.load();
  }


  search(): void {

    this.page.set(1);
    this.load();
  }


  reset(): void {

    this.filterForm.reset({
      search: '',
      status: '',
      sortBy: 'createdAt',
      descending: true,
      pageSize: 10
    });

    this.page.set(1);

    this.load();
  }


  previousPage(): void {

    if (!this.hasPreviousPage()) {
      return;
    }

    this.page.update(
      value => value - 1
    );

    this.load();
  }


  nextPage(): void {

    if (!this.hasNextPage()) {
      return;
    }

    this.page.update(
      value => value + 1
    );

    this.load();
  }


  changePageSize(): void {

    this.page.set(1);
    this.load();
  }


  private load(): void {

    if (this.loading()) {
      return;
    }

    this.loading.set(true);
    this.errorMessage.set(null);

    const filters =
      this.filterForm.getRawValue();

    this.api
      .getMine({
        search:
          filters.search,

        status:
          filters.status,

        sortBy:
          filters.sortBy,

        descending:
          filters.descending,

        page:
          this.page(),

        pageSize:
          filters.pageSize
      })
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

          this.result.set(
            response
          );

          this.page.set(
            response.page
          );
        },

        error: (error: unknown) => {

          this.result.set(null);

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
}