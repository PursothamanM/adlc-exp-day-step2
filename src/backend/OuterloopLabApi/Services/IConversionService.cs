using OuterloopLabApi.Domain;

namespace OuterloopLabApi.Services;

public interface IConversionService
{
    Task<ConversionResponse> ConvertAsync(ConversionRequest request, CancellationToken cancellationToken);
    Task<ConversionResponse?> GetAsync(string auditId, CancellationToken cancellationToken);
}
