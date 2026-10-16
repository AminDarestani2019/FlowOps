import { describe, expect, it } from 'vitest';

import {
  getWorkflowTaskSlaTiming
} from './workflow-task-sla';

import {
  WorkflowTask
} from './workflow-task.models';

function createTask(
  overrides: Partial<WorkflowTask> = {}
): WorkflowTask {
  return {
    taskKey: '1',
    name: 'Manager Approval',
    elementId: 'UserTask_ManagerApproval',
    state: 'CREATED',
    assignee: null,
    candidateGroups: ['procurement-managers'],
    candidateUsers: [],
    processDefinitionId: 'purchase-request-approval',
    processInstanceKey: '100',
    businessId: 'business-1',
    createdAt: '2026-10-16T10:00:00Z',
    dueDate: '2026-10-16T11:00:00Z',
    slaStatus: 'Due',
    priority: 50,
    ...overrides
  };
}

describe(
  'getWorkflowTaskSlaTiming',
  () => {
    it(
      'returns remaining time for a Due task',
      () => {
        const task = createTask();

        const result =
          getWorkflowTaskSlaTiming(
            task,
            Date.parse(
              '2026-10-16T10:15:00Z'
            )
          );

        expect(result)
          .toBe('45m remaining');
      }
    );

    it(
      'does not infer Overdue when backend still says Due',
      () => {
        const task = createTask();

        const result =
          getWorkflowTaskSlaTiming(
            task,
            Date.parse(
              '2026-10-16T11:01:00Z'
            )
          );

        expect(result)
          .toBe('Due now');
      }
    );

    it(
      'returns overdue duration when backend says Overdue',
      () => {
        const task =
          createTask({
            slaStatus: 'Overdue'
          });

        const result =
          getWorkflowTaskSlaTiming(
            task,
            Date.parse(
              '2026-10-16T12:12:00Z'
            )
          );

        expect(result)
          .toBe('1h 12m overdue');
      }
    );

    it(
      'uses backend Escalated status as source of truth',
      () => {
        const task =
          createTask({
            slaStatus: 'Escalated'
          });

        const result =
          getWorkflowTaskSlaTiming(
            task,
            Date.parse(
              '2026-10-16T09:00:00Z'
            )
          );

        expect(result)
          .toBe('Escalated');
      }
    );

    it(
      'returns null when task has no SLA',
      () => {
        const task =
          createTask({
            dueDate: null,
            slaStatus: null
          });

        expect(
          getWorkflowTaskSlaTiming(
            task,
            Date.now()
          )
        ).toBeNull();
      }
    );
  }
);
