namespace OuterloopLabApi.Domain;

public sealed class NormalizedRate
{
    public required decimal Rate { get; init; }
    public required string ProviderDate { get; init; }
    public string? ProviderSequence { get; init; }
}
