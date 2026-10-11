import {
  Routes
} from '@angular/router';

export const routes: Routes = [

  {
    path: '',
    redirectTo:
      'purchase-requests',
    pathMatch:
      'full'
  },
  {
    path: 'workflow/tasks',
    loadComponent: () =>
      import(
        './features/workflow/inbox/workflow-task-inbox'
      ).then(
        m => m.WorkflowTaskInboxComponent
      )
  },
  {
  path: 'workflow/tasks/:taskKey',
  loadComponent: () =>
    import(
      './features/workflow/task-detail/workflow-task-detail'
    ).then(
      m => m.WorkflowTaskDetailComponent
    )
  },
  {
    path:
      'purchase-requests',

    loadComponent: () =>
      import(
        './features/purchase-requests/list/purchase-request-list'
      )
        .then(
          m =>
            m.PurchaseRequestListComponent
        )
  },

  {
    path:
      'purchase-requests/new',

    loadComponent: () =>
      import(
        './features/purchase-requests/create/purchase-request-create'
      )
        .then(
          m =>
            m.PurchaseRequestCreateComponent
        )
  },
  {
    path:
      'purchase-requests/:id',

    loadComponent: () =>
      import(
        './features/purchase-requests/details/purchase-request-details'
      )
        .then(
          m =>
            m.PurchaseRequestDetailsComponent
        )
  },
  {
    path:
      '**',

    redirectTo:
      'purchase-requests'
  }

];