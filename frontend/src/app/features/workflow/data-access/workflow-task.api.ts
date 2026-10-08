import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { WorkflowTask } from './workflow-task.models';

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
  
}