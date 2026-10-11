export interface WorkflowTask {
  taskKey: string;
  name: string;
  state: string;
  assignee: string | null;
  processDefinitionId: string;
  processInstanceKey: string;
  businessId: string | null;
  createdAt: string | null;
  dueDate: string | null;
  priority: number;
}

export interface PurchaseRequestDecisionResponse {
  purchaseRequestId: string;
  status: string;
}