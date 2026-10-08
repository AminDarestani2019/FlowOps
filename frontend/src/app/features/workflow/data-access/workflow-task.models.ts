export interface WorkflowTask {
  taskKey: string;
  name: string;
  state: string;
  assignee: string | null;
  processDefinitionId: string;
  processInstanceKey: string;
  createdAt: string | null;
  dueDate: string | null;
  priority: number;
}