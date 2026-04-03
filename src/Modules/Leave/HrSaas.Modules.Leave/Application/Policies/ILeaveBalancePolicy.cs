namespace HrSaas.Modules.Leave.Application.Policies;

public interface ILeaveBalancePolicy
{
    int GetAnnualAllowance(int year);
    int GetSickAllowance(int year);
}
