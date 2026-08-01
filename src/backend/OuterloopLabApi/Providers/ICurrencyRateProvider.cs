using OuterloopLabApi.Domain;

namespace OuterloopLabApi.Providers;

public interface ICurrencyRateProvider
{
    Task<NormalizedRate> GetRateAsync(string fromCurrency, string toCurrency, CancellationToken cancellationToken);
}
