import {
  describe,
  expect,
  it
} from 'vitest';

import {
  WorkflowTask
} from './workflow-task.models';

import {
  getWorkflowTaskAssignmentAction
} from './workflow-task-assignment';

function createTask(
  overrides: Partial<WorkflowTask> = {}
): WorkflowTask {
  return {
    taskKey: '1',
    name: 'Manager Approval',
    elementId: 'UserTask_ManagerApproval',
    state: 'CREATED',
    assignee: null,
    candidateGroups: [
      'procurement-managers'
    ],
    candidateUsers: [],
    processDefinitionId:
      'purchase-request-approval',
    processInstanceKey: '100',
    businessId: 'business-1',
    createdAt:
      '2026-10-16T10:00:00Z',
    dueDate:
      '2026-10-17T10:00:00Z',
    slaStatus: 'Due',
    priority: 50,
    ...overrides
  };
}

describe(
  'getWorkflowTaskAssignmentAction',
  () => {
    it(
      'returns claim for an unassigned active task',
      () => {
        expect(
          getWorkflowTaskAssignmentAction(
            createTask()
          )
        ).toBe('claim');
      }
    );

    it(
      'returns unclaim for an assigned active task',
      () => {
        expect(
          getWorkflowTaskAssignmentAction(
            createTask({
              assignee:
                'flowops-dev-manager'
            })
          )
        ).toBe('unclaim');
      }
    );

    it(
      'returns null for a completed task',
      () => {
        expect(
          getWorkflowTaskAssignmentAction(
            createTask({
              state: 'COMPLETED'
            })
          )
        ).toBeNull();
      }
    );

    it(
      'treats state comparison case-insensitively',
      () => {
        expect(
          getWorkflowTaskAssignmentAction(
            createTask({
              state: 'created'
            })
          )
        ).toBe('claim');
      }
    );
  }
);
