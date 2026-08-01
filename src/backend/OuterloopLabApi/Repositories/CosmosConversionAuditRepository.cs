using Microsoft.Azure.Cosmos;
using OuterloopLabApi.Data;
using OuterloopLabApi.Domain;
using OuterloopLabApi.Settings;

namespace OuterloopLabApi.Repositories;

public sealed class CosmosConversionAuditRepository : IConversionAuditRepository
{
    private const string PartitionKeyPath = "/auditId";

    private readonly CosmosContainerFactory _factory;
    private readonly CosmosDbOptions _options;

    public CosmosConversionAuditRepository(CosmosDbOptions options)
    {
        _options = options;
        _factory = new CosmosContainerFactory(options);
    }

    public async Task<string> CreateAsync(ConversionAuditRecord record, CancellationToken cancellationToken)
    {
        var client = _factory.CreateCosmosClient();
        var container = _factory.CreateContainer(client);

        // CreateItemAsync ensures we don't overwrite an existing audit ID (append-only behavior).
        await container.CreateItemAsync(record, new PartitionKey(record.AuditId), cancellationToken: cancellationToken);
        return record.AuditId;
    }

    public async Task<ConversionAuditRecord?> GetAsync(string auditId, CancellationToken cancellationToken)
    {
        var client = _factory.CreateCosmosClient();
        var container = _factory.CreateContainer(client);

        try
        {
            var response = await container.ReadItemAsync<ConversionAuditRecord>(
                auditId,
                new PartitionKey(auditId),
                cancellationToken: cancellationToken);

            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }
}
