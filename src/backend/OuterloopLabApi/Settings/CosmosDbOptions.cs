namespace OuterloopLabApi.Settings;

public sealed class CosmosDbOptions
{
    public required string CosmosDbUri { get; init; }
    public required string DatabaseName { get; init; }
    public required string ContainerName { get; init; }
    public required string AccountName { get; init; }
    public required string ResourceGroupName { get; init; }
    public required string Region { get; init; }
    public required string ManagedIdentityClientId { get; init; }
}
