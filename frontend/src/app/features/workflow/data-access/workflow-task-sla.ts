import {
  WorkflowTask
} from './workflow-task.models';

export function getWorkflowTaskSlaTiming(
  task: WorkflowTask,
  nowMilliseconds: number
): string | null {
  if (!task.slaStatus) {
    return null;
  }

  if (task.slaStatus === 'Escalated') {
    return 'Escalated';
  }

  if (!task.dueDate) {
    return null;
  }

  const dueMilliseconds =
    Date.parse(task.dueDate);

  if (Number.isNaN(dueMilliseconds)) {
    return null;
  }

  const difference =
    dueMilliseconds - nowMilliseconds;

  if (task.slaStatus === 'Due') {
    if (difference <= 0) {
      return 'Due now';
    }

    return `${formatDuration(difference)} remaining`;
  }

  const overdueMilliseconds =
    Math.max(0, -difference);

  return `${formatDuration(overdueMilliseconds)} overdue`;
}

function formatDuration(
  milliseconds: number
): string {
  const totalMinutes =
    Math.max(
      0,
      Math.floor(
        milliseconds / 60_000
      )
    );

  const days =
    Math.floor(
      totalMinutes / 1_440
    );

  const hours =
    Math.floor(
      (totalMinutes % 1_440) / 60
    );

  const minutes =
    totalMinutes % 60;

  if (days > 0) {
    return `${days}d ${hours}h`;
  }

  if (hours > 0) {
    return `${hours}h ${minutes}m`;
  }

  if (totalMinutes > 0) {
    return `${minutes}m`;
  }

  return '< 1m';
}
