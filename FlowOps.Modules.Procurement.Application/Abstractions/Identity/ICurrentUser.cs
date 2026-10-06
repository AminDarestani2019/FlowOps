using FlowOps.Modules.Procurement.Domain.PurchaseRequests;

namespace FlowOps.Modules.Procurement.Application.Abstractions.Identity;

public interface ICurrentUser
{
    EmployeeId EmployeeId { get; }
}
