using HrSaas.Modules.Leave.Application.Policies;

namespace HrSaas.Modules.Leave.Infrastructure.Policies;

public sealed class DefaultLeaveBalancePolicy : ILeaveBalancePolicy
{
    public int GetAnnualAllowance(int year) => 20;
    public int GetSickAllowance(int year) => 10;
}
