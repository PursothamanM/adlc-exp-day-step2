using OuterloopLabApi.Domain;

namespace OuterloopLabApi.Repositories;

public interface IConversionAuditRepository
{
    Task<string> CreateAsync(ConversionAuditRecord record, CancellationToken cancellationToken);
    Task<ConversionAuditRecord?> GetAsync(string auditId, CancellationToken cancellationToken);
}
