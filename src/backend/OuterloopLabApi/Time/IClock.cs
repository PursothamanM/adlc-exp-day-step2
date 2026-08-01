namespace OuterloopLabApi.Time;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
