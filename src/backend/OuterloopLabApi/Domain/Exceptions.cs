namespace OuterloopLabApi.Domain;

public sealed class CurrencyRateProviderUnavailableException : Exception
{
    public CurrencyRateProviderUnavailableException(string message, Exception? inner = null) : base(message, inner)
    {
    }
}

public sealed class CurrencyRateNormalizationException : Exception
{
    public CurrencyRateNormalizationException(string message, Exception? inner = null) : base(message, inner)
    {
    }
}
