namespace HrSaas.SharedKernel.Audit;

public enum AuditAction
{
    Create = 1,
    Read = 2,
    Update = 3,
    Delete = 4,
    Login = 5,
    Logout = 6,
    Register = 7,
    Approve = 8,
    Reject = 9,
    Cancel = 10,
    Activate = 11,
    Suspend = 12,
    Reinstate = 13,
    Upgrade = 14,
    Assign = 15,
    Send = 16,
    Retry = 17,
    Export = 18,
    Import = 19,
    Upload = 20,
    Download = 21,
    SystemAction = 100
}
