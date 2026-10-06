using FlowOps.BuildingBlocks.Domain.Time;

namespace FlowOps.Modules.Procurement.Infrastructure.Time;

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
