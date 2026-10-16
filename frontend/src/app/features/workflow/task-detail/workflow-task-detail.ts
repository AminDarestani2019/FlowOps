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
  switchMap
} from 'rxjs';

import {
  WorkflowTaskApi
} from '../data-access/workflow-task.api';

import {
  WorkflowTask
} from '../data-access/workflow-task.models';

import {
  getWorkflowTaskSlaTiming
} from '../data-access/workflow-task-sla';

import {
  getWorkflowTaskAssignmentAction,
  WorkflowTaskAssignmentAction
} from '../data-access/workflow-task-assignment';

type TaskAction =
  | 'approve'
  | 'reject'
  | 'claim'
  | 'unclaim';

@Component({
  selector: 'app-workflow-task-detail',
  standalone: true,
  imports: [
    DatePipe
  ],
  templateUrl:
    './workflow-task-detail.html',
  styleUrl:
    './workflow-task-detail.scss'
})
export class WorkflowTaskDetailComponent {
  private readonly route =
    inject(ActivatedRoute);

  private readonly router =
    inject(Router);

  private readonly api =
    inject(WorkflowTaskApi);

  private readonly destroyRef =
    inject(DestroyRef);

  private readonly location =
    inject(Location);

  readonly task =
    signal<WorkflowTask | null>(null);

  readonly loading =
    signal(false);

  readonly error =
    signal<string | null>(null);

  readonly actionInProgress =
    signal<TaskAction | null>(null);

  readonly actionError =
    signal<string | null>(null);

  readonly nowMilliseconds =
    signal(Date.now());

  constructor() {
    this.loadTask();

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
      () =>
        window.clearInterval(
          timerId
        )
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

  assignmentAction(
    task: WorkflowTask
  ): WorkflowTaskAssignmentAction {
    return getWorkflowTaskAssignmentAction(
      task
    );
  }

  loadTask(): void {
    const taskKey =
      this.route
        .snapshot
        .paramMap
        .get('taskKey');

    if (!taskKey) {
      this.error.set(
        'Workflow task key is missing.'
      );

      return;
    }

    this.loading.set(true);
    this.error.set(null);

    this.api
      .getTask(taskKey)
      .pipe(
        takeUntilDestroyed(
          this.destroyRef
        )
      )
      .subscribe({
        next: task => {
          this.task.set(task);
          this.loading.set(false);
        },

        error: error => {
          console.error(error);

          if (error.status === 404) {
            this.error.set(
              'Workflow task was not found or is not available to the current user.'
            );
          } else {
            this.error.set(
              `Failed to load workflow task. HTTP ${
                error.status ?? 'unknown'
              }`
            );
          }

          this.loading.set(false);
        }
      });
  }

  claim(): void {
    this.executeAssignment(
      'claim'
    );
  }

  unclaim(): void {
    this.executeAssignment(
      'unclaim'
    );
  }

  approve(): void {
    this.executeDecision(
      'approve'
    );
  }

  reject(): void {
    this.executeDecision(
      'reject'
    );
  }

  private executeAssignment(
    action: 'claim' | 'unclaim'
  ): void {
    const currentTask =
      this.task();

    if (
      !currentTask
      || this.actionInProgress()
    ) {
      return;
    }

    this.actionError.set(null);

    this.actionInProgress.set(
      action
    );

    const request =
      action === 'claim'
        ? this.api.claimTask(
            currentTask.taskKey
          )
        : this.api.unclaimTask(
            currentTask.taskKey
          );

    request
      .pipe(
        switchMap(
          () =>
            this.api
              .waitForAssignmentState(
                currentTask.taskKey,
                action === 'claim'
              )
        ),
        takeUntilDestroyed(
          this.destroyRef
        )
      )
      .subscribe({
        next: task => {
          this.task.set(task);

          this.actionInProgress.set(
            null
          );
        },

        error: error => {
          console.error(error);

          this.actionInProgress.set(
            null
          );

          if (
            error.name ===
            'TimeoutError'
          ) {
            this.actionError.set(
              'Assignment command succeeded, but the workflow read model is still synchronizing.'
            );

            this.loadTask();
          } else if (
            error.status === 403
          ) {
            this.actionError.set(
              'You are not allowed to change this task assignment.'
            );
          } else if (
            error.status === 404
          ) {
            this.actionError.set(
              'The workflow task is no longer available.'
            );
          } else if (
            error.status === 409
          ) {
            this.actionError.set(
              'The task assignment changed before this action completed.'
            );

            this.loadTask();
          } else {
            this.actionError.set(
              `Failed to ${action} workflow task. HTTP ${
                error.status ?? 'unknown'
              }`
            );
          }
        }
      });
  }

  private executeDecision(
    decision: 'approve' | 'reject'
  ): void {
    const currentTask =
      this.task();

    if (!currentTask) {
      return;
    }

    if (this.actionInProgress()) {
      return;
    }

    if (
      currentTask.state.toUpperCase()
      !== 'CREATED'
    ) {
      this.actionError.set(
        'Workflow task is no longer active.'
      );

      return;
    }

    if (!currentTask.assignee) {
      this.actionError.set(
        'Claim this workflow task before completing it.'
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

    this.actionInProgress.set(
      decision
    );

    const request =
      decision === 'approve'
        ? this.api
            .approvePurchaseRequest(
              currentTask.businessId,
              currentTask.taskKey
            )
        : this.api
            .rejectPurchaseRequest(
              currentTask.businessId,
              currentTask.taskKey
            );

    request
      .pipe(
        takeUntilDestroyed(
          this.destroyRef
        )
      )
      .subscribe({
        next: () => {
          this.actionInProgress.set(
            null
          );

          void this.router.navigate([
            '/workflow/tasks'
          ]);
        },

        error: error => {
          console.error(error);

          this.actionInProgress.set(
            null
          );

          if (error.status === 404) {
            this.actionError.set(
              'Purchase request or workflow task was not found.'
            );
          } else if (
            error.status === 409
          ) {
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
