namespace HrSaas.Modules.Storage.Domain.Enums;

public enum FileStatus
{
    Active = 0,
    Archived = 1,
    PendingDeletion = 2,
    Orphaned = 3
}
