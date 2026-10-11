import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { WorkflowTask ,PurchaseRequestDecisionResponse} from './workflow-task.models';

@Injectable({
  providedIn: 'root'
})
export class WorkflowTaskApi {
  private readonly http = inject(HttpClient);

  getMyTasks(): Observable<WorkflowTask[]> {
    return this.http.get<WorkflowTask[]>(
      '/api/workflow/tasks'
    );
  }

  getTask(taskKey: string): Observable<WorkflowTask> {
    return this.http.get<WorkflowTask>(
      `/api/workflow/tasks/${taskKey}`
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