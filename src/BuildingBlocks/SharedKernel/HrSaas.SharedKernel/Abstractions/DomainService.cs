using Microsoft.Extensions.Logging;

namespace HrSaas.SharedKernel.Abstractions;

public abstract class DomainService(ILogger<DomainService> logger)
{
    protected ILogger Logger { get; } = logger;
}
