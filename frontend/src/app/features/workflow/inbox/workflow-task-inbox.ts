import {
  Component,
  DestroyRef,
  inject,
  signal
} from '@angular/core';

import {
  DatePipe
} from '@angular/common';

import {
  RouterLink
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

@Component({
  selector: 'app-workflow-task-inbox',
  standalone: true,
  imports: [
    DatePipe,
    RouterLink
  ],
  templateUrl: './workflow-task-inbox.html',
  styleUrl: './workflow-task-inbox.scss'
})
export class WorkflowTaskInboxComponent {
  private readonly api =
    inject(WorkflowTaskApi);

  private readonly destroyRef =
    inject(DestroyRef);

  readonly tasks =
    signal<WorkflowTask[]>([]);

  readonly loading =
    signal(false);

  readonly error =
    signal<string | null>(null);

  readonly assignmentError =
    signal<string | null>(null);

  readonly assignmentInProgress =
    signal<string | null>(null);

  readonly nowMilliseconds =
    signal(Date.now());

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

  claim(
    task: WorkflowTask
  ): void {
    this.executeAssignment(
      task,
      'claim'
    );
  }

  unclaim(
    task: WorkflowTask
  ): void {
    this.executeAssignment(
      task,
      'unclaim'
    );
  }

  loadTasks(): void {
    this.loading.set(true);
    this.error.set(null);

    this.api
      .getMyTasks()
      .pipe(
        takeUntilDestroyed(
          this.destroyRef
        )
      )
      .subscribe({
        next: tasks => {
          this.tasks.set(tasks);
          this.loading.set(false);
        },

        error: error => {
          console.error(error);

          this.error.set(
            `Failed to load workflow tasks. HTTP ${
              error.status ?? 'unknown'
            }`
          );

          this.loading.set(false);
        }
      });
  }

  private executeAssignment(
    task: WorkflowTask,
    action: 'claim' | 'unclaim'
  ): void {
    if (
      this.assignmentInProgress()
    ) {
      return;
    }

    this.assignmentError.set(null);

    this.assignmentInProgress.set(
      task.taskKey
    );

    const request =
      action === 'claim'
        ? this.api.claimTask(
            task.taskKey
          )
        : this.api.unclaimTask(
            task.taskKey
          );

    request
      .pipe(
        switchMap(
          () =>
            this.api
              .waitForAssignmentState(
                task.taskKey,
                action === 'claim'
              )
        ),
        takeUntilDestroyed(
          this.destroyRef
        )
      )
      .subscribe({
        next: () => {
          this.assignmentInProgress.set(
            null
          );

          this.loadTasks();
        },

        error: error => {
          console.error(error);

          this.assignmentInProgress.set(
            null
          );

          if (
            error.name ===
            'TimeoutError'
          ) {
            this.assignmentError.set(
              'Assignment command succeeded, but the workflow read model is still synchronizing.'
            );
          } else if (
            error.status === 403
          ) {
            this.assignmentError.set(
              'You are not allowed to change this task assignment.'
            );
          } else if (
            error.status === 404
          ) {
            this.assignmentError.set(
              'The workflow task is no longer available.'
            );
          } else if (
            error.status === 409
          ) {
            this.assignmentError.set(
              'The task assignment changed before this action completed.'
            );
          } else {
            this.assignmentError.set(
              `Failed to ${action} workflow task. HTTP ${
                error.status ?? 'unknown'
              }`
            );
          }

          this.loadTasks();
        }
      });
  }
}
