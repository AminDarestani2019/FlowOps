import {
  Component,
  DestroyRef,
  inject,
  signal
} from '@angular/core';

import {
  DatePipe,
  Location
} from '@angular/common';

import {
  ActivatedRoute,
  Router
} from '@angular/router';

import {
  takeUntilDestroyed
} from '@angular/core/rxjs-interop';

import {
  WorkflowTaskApi
} from '../data-access/workflow-task.api';

import {
  WorkflowTask
} from '../data-access/workflow-task.models';

@Component({
  selector: 'app-workflow-task-detail',
  standalone: true,
  imports: [
    DatePipe
  ],
  templateUrl: './workflow-task-detail.html',
  styleUrl: './workflow-task-detail.scss'
})
export class WorkflowTaskDetailComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly api = inject(WorkflowTaskApi);
  private readonly destroyRef = inject(DestroyRef);
  private readonly location = inject(Location);

  readonly task = signal<WorkflowTask | null>(null);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  readonly actionInProgress = signal<'approve' | 'reject' | null>(null);
  readonly actionError = signal<string | null>(null);


  constructor() {
    this.loadTask();
  }

  loadTask(): void {
    const taskKey =
      this.route.snapshot.paramMap.get('taskKey');

    if (!taskKey) {
      this.error.set('Workflow task key is missing.');
      return;
    }

    this.loading.set(true);
    this.error.set(null);

    this.api
      .getTask(taskKey)
      .pipe(
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: task => {
          this.task.set(task);
          this.loading.set(false);
        },

        error: error => {
          console.error(error);

          if (error.status === 404) {
            this.error.set('Workflow task was not found.');
          } else {
            this.error.set(
              `Failed to load workflow task. HTTP ${error.status ?? 'unknown'}`
            );
          }

          this.loading.set(false);
        }
      });
  }


  approve(): void {
    this.executeDecision('approve');
  }

  reject(): void {
    this.executeDecision('reject');
  }

  private executeDecision(
    decision: 'approve' | 'reject'
  ): void {
    const currentTask = this.task();

    if (!currentTask) {
      return;
    }

    if (this.actionInProgress()) {
      return;
    }

    if (
      currentTask.state.toUpperCase() !==
      'CREATED'
    ) {
      this.actionError.set(
        'Workflow task is no longer active.'
      );

      return;
    }

    if (!currentTask.businessId) {
      this.actionError.set(
        'Workflow task does not contain a business correlation ID.'
      );

      return;
    }

    this.actionError.set(null);
    this.actionInProgress.set(decision);

    const request =
      decision === 'approve'
        ? this.api.approvePurchaseRequest(
            currentTask.businessId,
            currentTask.taskKey
          )
        : this.api.rejectPurchaseRequest(
            currentTask.businessId,
            currentTask.taskKey
          );

    request
      .pipe(
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: () => {
          this.actionInProgress.set(null);

          void this.router.navigate([
            '/workflow/tasks'
          ]);
        },

        error: error => {
          console.error(error);

          this.actionInProgress.set(null);

          if (error.status === 404) {
            this.actionError.set(
              'Purchase request or workflow task was not found.'
            );
          } else if (error.status === 409) {
            this.actionError.set(
              error.error?.detail ??
              'The request cannot be completed because its state has changed.'
            );
          } else {
            this.actionError.set(
              `Failed to ${
                decision === 'approve'
                  ? 'approve'
                  : 'reject'
              } purchase request. HTTP ${
                error.status ?? 'unknown'
              }`
            );
          }
        }
      });
  }

  back(): void {
    this.location.back();
  }
}