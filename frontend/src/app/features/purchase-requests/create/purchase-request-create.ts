import {
  Component,
  computed,
  DestroyRef,
  inject,
  signal
} from '@angular/core';

import {
  FormArray,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';

import {
  HttpErrorResponse
} from '@angular/common/http';

import {
  takeUntilDestroyed,
  toSignal
} from '@angular/core/rxjs-interop';

import {
  finalize,
  startWith
} from 'rxjs';

import {
  PurchaseRequestApi
} from '../data-access/purchase-request.api';

import {
  AddPurchaseRequestItemResponse,
  CreatePurchaseRequestResponse
} from '../data-access/purchase-request.models';


type ItemForm = FormGroup<{
  description: FormControl<string>;
  quantity: FormControl<number>;
  unitPrice: FormControl<number>;
}>;


@Component({
  selector: 'app-purchase-request-create',
  standalone: true,
  imports: [
    ReactiveFormsModule
  ],
  templateUrl: './purchase-request-create.html',
  styleUrl: './purchase-request-create.scss'
})
export class PurchaseRequestCreateComponent {

  private readonly api =
    inject(PurchaseRequestApi);

  private readonly destroyRef =
    inject(DestroyRef);


  // -------------------------------------------------------
  // Create Purchase Request
  // -------------------------------------------------------

  readonly creatingRequest =
    signal(false);

  readonly createError =
    signal<string | null>(null);

  readonly createdRequest =
    signal<CreatePurchaseRequestResponse | null>(null);


  readonly createForm = new FormGroup({

    departmentId: new FormControl(
      '22222222-2222-2222-2222-222222222222',
      {
        nonNullable: true,
        validators: [
          Validators.required,
          Validators.pattern(
            /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i
          )
        ]
      }
    ),

    currency: new FormControl(
      'EUR',
      {
        nonNullable: true,
        validators: [
          Validators.required
        ]
      }
    )

  });


  // -------------------------------------------------------
  // Purchase Request Items
  // -------------------------------------------------------

  readonly items =
    new FormArray<ItemForm>([
      this.createItemForm()
    ]);


  private readonly itemValues =
    toSignal(
      this.items.valueChanges.pipe(
        startWith(this.items.getRawValue())
      ),
      {
        initialValue:
          this.items.getRawValue()
      }
    );


  readonly formTotal =
    computed(() => {

      const items =
        this.itemValues();

      return items.reduce(
        (total, item) =>
          total +
          (
            Number(item.quantity ?? 0) *
            Number(item.unitPrice ?? 0)
          ),
        0
      );
    });


  readonly savingItemIndex =
    signal<number | null>(null);


  readonly persistedItemIds =
    signal<(string | null)[]>([
      null
    ]);


  readonly persistedTotal =
    signal(0);


  readonly itemError =
    signal<string | null>(null);


  readonly lastSavedItem =
    signal<AddPurchaseRequestItemResponse | null>(
      null
    );


  // -------------------------------------------------------
  // Create Request
  // -------------------------------------------------------

  submit(): void {

    if (this.creatingRequest()) {
      return;
    }

    if (this.createForm.invalid) {
      this.createForm.markAllAsTouched();
      return;
    }

    this.creatingRequest.set(true);
    this.createError.set(null);

    const request =
      this.createForm.getRawValue();

    this.api
      .create(request)
      .pipe(
        takeUntilDestroyed(this.destroyRef),

        finalize(() =>
          this.creatingRequest.set(false)
        )
      )
      .subscribe({

        next: response => {

          this.createdRequest.set(
            response
          );

          this.persistedTotal.set(0);
          this.itemError.set(null);
        },

        error: (error: unknown) => {

          this.createError.set(
            this.getErrorMessage(error)
          );
        }
      });
  }


  // -------------------------------------------------------
  // Dynamic Items
  // -------------------------------------------------------

  addItemRow(): void {

    this.items.push(
      this.createItemForm()
    );

    this.persistedItemIds.update(
      ids => [
        ...ids,
        null
      ]
    );
  }


  removeItemRow(index: number): void {

    const persistedId =
      this.persistedItemIds()[index];

    if (persistedId) {

      this.itemError.set(
        'This item is already persisted. ' +
        'Deleting persisted items will be implemented ' +
        'in the RemovePurchaseRequestItem slice.'
      );

      return;
    }

    if (this.items.length === 1) {
      return;
    }

    this.items.removeAt(index);

    this.persistedItemIds.update(
      ids =>
        ids.filter(
          (_, currentIndex) =>
            currentIndex !== index
        )
    );
  }


  saveItem(index: number): void {

    const purchaseRequest =
      this.createdRequest();

    if (!purchaseRequest) {

      this.itemError.set(
        'Create the purchase request first.'
      );

      return;
    }

    const form =
      this.items.at(index);

    if (form.invalid) {

      form.markAllAsTouched();
      return;
    }

    if (this.persistedItemIds()[index]) {
      return;
    }

    if (this.savingItemIndex() !== null) {
      return;
    }

    this.savingItemIndex.set(index);
    this.itemError.set(null);

    const request =
      form.getRawValue();

    this.api
      .addItem(
        purchaseRequest.id,
        request
      )
      .pipe(

        takeUntilDestroyed(
          this.destroyRef
        ),

        finalize(() =>
          this.savingItemIndex.set(null)
        )
      )
      .subscribe({

        next: response => {

          this.persistedItemIds.update(
            ids => {

              const copy =
                [...ids];

              copy[index] =
                response.itemId;

              return copy;
            }
          );

          this.persistedTotal.set(
            response.requestTotal
          );

          this.lastSavedItem.set(
            response
          );

          form.disable();
        },

        error: (error: unknown) => {

          this.itemError.set(
            this.getErrorMessage(error)
          );
        }
      });
  }


  // -------------------------------------------------------
  // Helpers
  // -------------------------------------------------------

  lineTotal(index: number): number {

    const value =
      this.items.at(index)
        .getRawValue();

    return (
      Number(value.quantity) *
      Number(value.unitPrice)
    );
  }


  isPersisted(index: number): boolean {

    return Boolean(
      this.persistedItemIds()[index]
    );
  }


  private createItemForm(): ItemForm {

    return new FormGroup({

      description:
        new FormControl(
          '',
          {
            nonNullable: true,
            validators: [
              Validators.required,
              Validators.maxLength(500)
            ]
          }
        ),

      quantity:
        new FormControl(
          1,
          {
            nonNullable: true,
            validators: [
              Validators.required,
              Validators.min(1)
            ]
          }
        ),

      unitPrice:
        new FormControl(
          0,
          {
            nonNullable: true,
            validators: [
              Validators.required,
              Validators.min(0)
            ]
          }
        )

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