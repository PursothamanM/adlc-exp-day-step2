using OuterloopLabApi.Domain;
using OuterloopLabApi.Providers;
using OuterloopLabApi.Repositories;
using OuterloopLabApi.Time;

namespace OuterloopLabApi.Services;

public sealed class ConversionService : IConversionService
{
    private static readonly System.Text.RegularExpressions.Regex CurrencyRegex = new("^[A-Z]{3}$", System.Text.RegularExpressions.RegexOptions.Compiled);

    private readonly ICurrencyRateProvider _rateProvider;
    private readonly IConversionAuditRepository _repository;
    private readonly IClock _clock;

    public ConversionService(
        ICurrencyRateProvider rateProvider,
        IConversionAuditRepository repository,
        IClock clock)
    {
        _rateProvider = rateProvider;
        _repository = repository;
        _clock = clock;
    }

    public async Task<ConversionResponse> ConvertAsync(ConversionRequest request, CancellationToken cancellationToken)
    {
        if (!CurrencyRegex.IsMatch(request.FromCurrency))
            throw new ArgumentException("Invalid fromCurrency", nameof(request));

        if (!CurrencyRegex.IsMatch(request.ToCurrency))
            throw new ArgumentException("Invalid toCurrency", nameof(request));

        if (request.Amount <= 0)
            throw new ArgumentException("Amount must be greater than 0", nameof(request));

        var normalized = await _rateProvider.GetRateAsync(request.FromCurrency, request.ToCurrency, cancellationToken);

        var convertedAmount = request.Amount * normalized.Rate;
        var executedAtUtc = _clock.UtcNow;

        var auditId = Guid.NewGuid().ToString("N");

        var record = new ConversionAuditRecord
        {
            Id = auditId,
            AuditId = auditId,
            Amount = request.Amount,
            FromCurrency = request.FromCurrency,
            ToCurrency = request.ToCurrency,
            Rate = normalized.Rate,
            ConvertedAmount = decimal.Round(convertedAmount, 4),
            ProviderDate = normalized.ProviderDate,
            ProviderSequence = normalized.ProviderSequence,
            ExecutedAtUtc = executedAtUtc,
        };

        await _repository.CreateAsync(record, cancellationToken);

        return new ConversionResponse
        {
            AuditId = auditId,
            Amount = record.Amount,
            FromCurrency = record.FromCurrency,
            ToCurrency = record.ToCurrency,
            Rate = record.Rate,
            ConvertedAmount = record.ConvertedAmount,
            ProviderDate = record.ProviderDate,
            ExecutedAtUtc = record.ExecutedAtUtc.ToString("O"),
        };
    }

    public async Task<ConversionResponse?> GetAsync(string auditId, CancellationToken cancellationToken)
    {
        var record = await _repository.GetAsync(auditId, cancellationToken);
        if (record is null) return null;

        return new ConversionResponse
        {
            AuditId = record.AuditId,
            Amount = record.Amount,
            FromCurrency = record.FromCurrency,
            ToCurrency = record.ToCurrency,
            Rate = record.Rate,
            ConvertedAmount = record.ConvertedAmount,
            ProviderDate = record.ProviderDate,
            ExecutedAtUtc = record.ExecutedAtUtc.ToString("O"),
        };
    }
}
