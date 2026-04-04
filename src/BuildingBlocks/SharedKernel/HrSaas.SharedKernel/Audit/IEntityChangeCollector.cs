namespace HrSaas.SharedKernel.Audit;

public interface IEntityChangeCollector
{
    IReadOnlyList<EntityChangeEntry> Collect();

    void Clear();
}
