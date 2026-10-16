import {
  inject,
  Injectable
} from '@angular/core';

import {
  HttpClient
} from '@angular/common/http';

import {
  filter,
  Observable,
  switchMap,
  take,
  timer,
  timeout
} from 'rxjs';

import {
  PurchaseRequestDecisionResponse,
  WorkflowTask
} from './workflow-task.models';

@Injectable({
  providedIn: 'root'
})
export class WorkflowTaskApi {
  private readonly http =
    inject(HttpClient);

  getMyTasks():
    Observable<WorkflowTask[]> {
    return this.http.get<WorkflowTask[]>(
      '/api/workflow/tasks'
    );
  }

  getTask(
    taskKey: string
  ): Observable<WorkflowTask> {
    return this.http.get<WorkflowTask>(
      `/api/workflow/tasks/${taskKey}`
    );
  }

  claimTask(
    taskKey: string
  ): Observable<void> {
    return this.http.post<void>(
      `/api/workflow/tasks/${taskKey}/claim`,
      null
    );
  }

  unclaimTask(
    taskKey: string
  ): Observable<void> {
    return this.http.post<void>(
      `/api/workflow/tasks/${taskKey}/unclaim`,
      null
    );
  }

  waitForAssignmentState(
    taskKey: string,
    assigned: boolean
  ): Observable<WorkflowTask> {
    return timer(
      0,
      250
    ).pipe(
      switchMap(
        () => this.getTask(taskKey)
      ),
      filter(
        task =>
          assigned
            ? !!task.assignee
            : !task.assignee
      ),
      take(1),
      timeout(10_000)
    );
  }

  approvePurchaseRequest(
    purchaseRequestId: string,
    taskKey: string
  ): Observable<PurchaseRequestDecisionResponse> {
    return this.http.post<PurchaseRequestDecisionResponse>(
      `/api/procurement/purchase-requests/${purchaseRequestId}/approve`,
      {
        taskKey
      }
    );
  }

  rejectPurchaseRequest(
    purchaseRequestId: string,
    taskKey: string
  ): Observable<PurchaseRequestDecisionResponse> {
    return this.http.post<PurchaseRequestDecisionResponse>(
      `/api/procurement/purchase-requests/${purchaseRequestId}/reject`,
      {
        taskKey
      }
    );
  }
}
