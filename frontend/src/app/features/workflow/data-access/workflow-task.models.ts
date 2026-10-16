export type WorkflowTaskSlaStatus =
  | 'Due'
  | 'Overdue'
  | 'Escalated'
  | null;

export interface WorkflowTask {
  taskKey: string;
  name: string;
  elementId: string;
  state: string;
  assignee: string | null;
  candidateGroups: string[];
  candidateUsers: string[];
  processDefinitionId: string;
  processInstanceKey: string;
  businessId: string | null;
  createdAt: string | null;
  dueDate: string | null;
  slaStatus: WorkflowTaskSlaStatus;
  priority: number;
}

export interface PurchaseRequestDecisionResponse {
  purchaseRequestId: string;
  status: string;
}
