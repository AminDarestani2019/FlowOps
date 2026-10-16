import {
  Component,
  DestroyRef,
  inject,
  signal
} from '@angular/core';

import { RouterLink } from '@angular/router';

import { DatePipe } from '@angular/common';

import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { WorkflowTaskApi } from '../data-access/workflow-task.api';
import { WorkflowTask } from '../data-access/workflow-task.models';
import {
  getWorkflowTaskSlaTiming
} from '../data-access/workflow-task-sla';

@Component({
  selector: 'app-workflow-task-inbox',
  standalone: true,
  imports: [
    DatePipe, RouterLink
  ],
  templateUrl: './workflow-task-inbox.html',
  styleUrl: './workflow-task-inbox.scss'
})
export class WorkflowTaskInboxComponent {
  private readonly api = inject(WorkflowTaskApi);
  private readonly destroyRef = inject(DestroyRef);

  readonly tasks = signal<WorkflowTask[]>([]);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly nowMilliseconds = signal(Date.now());

  constructor() {
    this.loadTasks();

    const timerId =
      window.setInterval(
        () => {
          this.nowMilliseconds.set(
            Date.now()
          );
        },
        1_000
      );

    this.destroyRef.onDestroy(
      () => window.clearInterval(timerId)
    );
  }

  slaTiming(
    task: WorkflowTask
  ): string | null {
    return getWorkflowTaskSlaTiming(
      task,
      this.nowMilliseconds()
    );
  }

  loadTasks(): void {
    this.loading.set(true);
    this.error.set(null);

    this.api
      .getMyTasks()
      .pipe(
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: tasks => {
          this.tasks.set(tasks);
          this.loading.set(false);
        },

        error: error => {
          console.error(error);

          this.error.set(
            `Failed to load workflow tasks. HTTP ${error.status ?? 'unknown'}`
          );

          this.loading.set(false);
        }
      });
  }
}