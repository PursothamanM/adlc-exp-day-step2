using System.Net;
using System.Text.Json;
using OuterloopLabApi.Domain;

namespace OuterloopLabApi.Providers;

public sealed class FrankfurterCurrencyRateProvider : ICurrencyRateProvider
{
    private readonly HttpClient _httpClient;

    public FrankfurterCurrencyRateProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<NormalizedRate> GetRateAsync(string fromCurrency, string toCurrency, CancellationToken cancellationToken)
    {
        var baseUrl = Environment.GetEnvironmentVariable("CURRENCY_API_BASE_URL") ?? "https://frankfurter.dev";
        var baseUrlNormalized = baseUrl.TrimEnd('/');

        Exception? lastError = null;

        // Spec constraint: use *exclusively* the configured base URL.
        // If the configured base supports v2, we use it; otherwise v1 acts as a fallback.
        try
        {
            // Frankfurter v2 single rate
            var v2Url = $"{baseUrlNormalized}/v2/rate/{fromCurrency}/{toCurrency}";
            var v2 = await TryGetJsonAsync(v2Url, cancellationToken);
            if (TryNormalizeRate(v2, toCurrency, out var normalizedV2))
                return normalizedV2;
        }
        catch (Exception ex)
        {
            lastError = ex;
        }

        try
        {
            // Frankfurter v1 latest rates (map-style)
            var v1Url = $"{baseUrlNormalized}/v1/latest?base={fromCurrency}";
            var v1 = await TryGetJsonAsync(v1Url, cancellationToken);
            if (TryNormalizeRate(v1, toCurrency, out var normalizedV1))
                return normalizedV1;
        }
        catch (Exception ex)
        {
            lastError = ex;
        }

        throw new CurrencyRateProviderUnavailableException("Currency rate provider unavailable", lastError);
    }

    private async Task<JsonDocument> TryGetJsonAsync(string url, CancellationToken cancellationToken)
    {
        HttpResponseMessage res;
        try
        {
            res = await _httpClient.GetAsync(url, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new CurrencyRateProviderUnavailableException("Currency rate provider unavailable", ex);
        }

        if (!res.IsSuccessStatusCode)
            throw new CurrencyRateProviderUnavailableException($"Currency rate provider unavailable (HTTP {(int)res.StatusCode})");

        try
        {
            var stream = await res.Content.ReadAsStreamAsync(cancellationToken);
            return await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            throw new CurrencyRateNormalizationException("Failed to parse provider JSON", ex);
        }
    }

    private static bool TryNormalizeRate(JsonDocument json, string targetCurrency, out NormalizedRate normalized)
    {
        normalized = null!;

        // Flexible normalization: locate provider date and either a direct `rate` field or a map of target rates.
        var root = json.RootElement;

        string? providerDate = null;
        if (root.TryGetProperty("date", out var dateProp) && dateProp.ValueKind == JsonValueKind.String)
            providerDate = dateProp.GetString();

        decimal? rate = null;

        if (root.TryGetProperty("rate", out var directRate) && directRate.ValueKind is JsonValueKind.Number)
            rate = directRate.GetDecimal();

        if (rate is null)
        {
            // Spec mentions schema differences like `rates` vs `conversion_rates`.
            if (TryReadRateFromMap(root, "rates", targetCurrency, out var mappedRate))
                rate = mappedRate;
            else if (TryReadRateFromMap(root, "conversion_rates", targetCurrency, out var mappedRate2))
                rate = mappedRate2;
        }

        if (rate is null || string.IsNullOrWhiteSpace(providerDate))
            return false;

        root.TryGetProperty("sequence", out var seqProp);
        string? providerSequence = root.TryGetProperty("sequence", out var seq) && seq.ValueKind == JsonValueKind.String
            ? seq.GetString()
            : null;

        normalized = new NormalizedRate
        {
            Rate = rate.Value,
            ProviderDate = providerDate,
            ProviderSequence = providerSequence,
        };
        return true;
    }

    private static bool TryReadRateFromMap(JsonElement root, string mapPropertyName, string targetCurrency, out decimal mappedRate)
    {
        mappedRate = 0;
        if (!root.TryGetProperty(mapPropertyName, out var map) || map.ValueKind != JsonValueKind.Object)
            return false;

        if (!map.TryGetProperty(targetCurrency, out var rateProp) || rateProp.ValueKind is not JsonValueKind.Number)
            return false;

        mappedRate = rateProp.GetDecimal();
        return true;
    }
}
