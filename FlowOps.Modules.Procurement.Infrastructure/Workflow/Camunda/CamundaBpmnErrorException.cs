namespace FlowOps.Modules.Procurement.Infrastructure.Workflow.Camunda;

internal sealed class CamundaBpmnErrorException : Exception
{
    public CamundaBpmnErrorException(
        string errorCode,
        string message)
        : base(message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(errorCode);

        ErrorCode = errorCode;
    }

    public string ErrorCode { get; }
}