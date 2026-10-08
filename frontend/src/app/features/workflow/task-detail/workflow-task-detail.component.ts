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
  ActivatedRoute
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
  templateUrl: './workflow-task-detail.component.html',
  styleUrl: './workflow-task-detail.component.scss'
})
export class WorkflowTaskDetailComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly api = inject(WorkflowTaskApi);
  private readonly destroyRef = inject(DestroyRef);
  private readonly location = inject(Location);

  readonly task = signal<WorkflowTask | null>(null);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

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

  back(): void {
    this.location.back();
  }
}