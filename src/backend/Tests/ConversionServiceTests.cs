using OuterloopLabApi.Domain;
using OuterloopLabApi.Providers;
using OuterloopLabApi.Repositories;
using OuterloopLabApi.Services;
using OuterloopLabApi.Time;

namespace Tests;

public sealed class ConversionServiceTests
{
    private sealed class FakeClock : IClock
    {
        public FakeClock(DateTimeOffset now) => UtcNow = now;
        public DateTimeOffset UtcNow { get; }
    }

    private sealed class InMemoryRepo : IConversionAuditRepository
    {
        private readonly Dictionary<string, ConversionAuditRecord> _store = new();
        public Task<string> CreateAsync(ConversionAuditRecord record, CancellationToken cancellationToken)
        {
            _store[record.AuditId] = record;
            return Task.FromResult(record.AuditId);
        }

        public Task<ConversionAuditRecord?> GetAsync(string auditId, CancellationToken cancellationToken)
        {
            _store.TryGetValue(auditId, out var record);
            return Task.FromResult(record);
        }
    }

    private sealed class FakeProvider : ICurrencyRateProvider
    {
        private readonly NormalizedRate _rate;
        private readonly Exception? _exception;

        public FakeProvider(NormalizedRate rate)
        {
            _rate = rate;
        }

        public FakeProvider(Exception exception)
        {
            _exception = exception;
            _rate = null!;
        }

        public Task<NormalizedRate> GetRateAsync(string fromCurrency, string toCurrency, CancellationToken cancellationToken)
        {
            if (_exception is not null) throw _exception;
            return Task.FromResult(_rate);
        }
    }

    [Fact]
    public async Task ConvertAsync_success_persists_and_returns_audit_response()
    {
        var fixedNow = new DateTimeOffset(2026, 1, 15, 14, 3, 27, TimeSpan.Zero);
        var provider = new FakeProvider(new NormalizedRate
        {
            Rate = 0.92m,
            ProviderDate = "2026-01-15",
        });
        var repo = new InMemoryRepo();
        var clock = new FakeClock(fixedNow);

        var service = new ConversionService(provider, repo, clock);
        var req = new ConversionRequest { Amount = 100m, FromCurrency = "USD", ToCurrency = "EUR" };

        var resp = await service.ConvertAsync(req, CancellationToken.None);

        Assert.False(string.IsNullOrWhiteSpace(resp.AuditId));
        Assert.Equal(100m, resp.Amount);
        Assert.Equal("USD", resp.FromCurrency);
        Assert.Equal("EUR", resp.ToCurrency);
        Assert.Equal(0.92m, resp.Rate);
        Assert.Equal(92.0000m, resp.ConvertedAmount);
        Assert.Equal("2026-01-15", resp.ProviderDate);
        Assert.Equal(fixedNow.ToString("O"), resp.ExecutedAtUtc);
    }

    [Fact]
    public async Task ConvertAsync_upstream_failure_does_not_persist()
    {
        var repo = new InMemoryRepo();
        var provider = new FakeProvider(new CurrencyRateProviderUnavailableException("Currency rate provider unavailable"));
        var clock = new FakeClock(DateTimeOffset.UtcNow);

        var service = new ConversionService(provider, repo, clock);
        var req = new ConversionRequest { Amount = 100m, FromCurrency = "USD", ToCurrency = "EUR" };

        await Assert.ThrowsAsync<CurrencyRateProviderUnavailableException>(() => service.ConvertAsync(req, CancellationToken.None));
    }
}
