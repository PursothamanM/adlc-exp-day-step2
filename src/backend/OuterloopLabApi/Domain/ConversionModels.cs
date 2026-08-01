using System.Text.Json.Serialization;

namespace OuterloopLabApi.Domain;

public sealed class ConversionRequest
{
    public decimal Amount { get; set; }
    public string FromCurrency { get; set; } = string.Empty;
    public string ToCurrency { get; set; } = string.Empty;
}

public sealed class ConversionResponse
{
    public required string AuditId { get; init; }
    public required decimal Amount { get; init; }
    public required string FromCurrency { get; init; }
    public required string ToCurrency { get; init; }
    public required decimal Rate { get; init; }
    public required decimal ConvertedAmount { get; init; }
    public required string ProviderDate { get; init; }
    public required string ExecutedAtUtc { get; init; }
}

public sealed class ConversionAuditRecord
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    // Partition key to make point reads cheap and deterministic.
    [JsonPropertyName("auditId")]
    public required string AuditId { get; init; }

    public required decimal Amount { get; init; }
    public required string FromCurrency { get; init; }
    public required string ToCurrency { get; init; }
    public required decimal Rate { get; init; }
    public required decimal ConvertedAmount { get; init; }
    public required string ProviderDate { get; init; }
    public string? ProviderSequence { get; init; }

    // Exact backend server execution timestamp (UTC, ISO-8601 with fractional seconds).
    public required DateTimeOffset ExecutedAtUtc { get; init; }
}
