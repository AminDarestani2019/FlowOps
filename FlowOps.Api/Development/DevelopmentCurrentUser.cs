using FlowOps.Modules.Procurement.Application.Abstractions.Identity;
using FlowOps.Modules.Procurement.Domain.PurchaseRequests;

namespace FlowOps.Api.Development;

public sealed class DevelopmentCurrentUser: ICurrentUser
{
    public EmployeeId EmployeeId { get; } =
      new EmployeeId(
          Guid.Parse(
              "11111111-1111-1111-1111-111111111111"));
}
