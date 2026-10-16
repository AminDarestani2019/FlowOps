import {
  WorkflowTask
} from './workflow-task.models';

export type WorkflowTaskAssignmentAction =
  | 'claim'
  | 'unclaim'
  | null;

export function getWorkflowTaskAssignmentAction(
  task: WorkflowTask
): WorkflowTaskAssignmentAction {
  if (task.state.toUpperCase() !== 'CREATED') {
    return null;
  }

  return task.assignee
    ? 'unclaim'
    : 'claim';
}
