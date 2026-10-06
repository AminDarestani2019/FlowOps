import { Routes } from '@angular/router';

export const routes: Routes = [
    {
    path: '',
    redirectTo: 'purchase-requests/new',
    pathMatch: 'full'
  },
  {
    path: 'purchase-requests/new',
    loadComponent: () =>
      import(
        './features/purchase-requests/create/purchase-request-create'
      ).then(m => m.PurchaseRequestCreateComponent)
  },
  {
    path: '**',
    redirectTo: 'purchase-requests/new'
  }
];
